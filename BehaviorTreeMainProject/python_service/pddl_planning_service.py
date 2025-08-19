#!/usr/bin/env python3
"""
PDDL Planning Service
REST API service that calls ENHSP planner for PDDL planning
"""

from flask import Flask, request, jsonify
import subprocess
import tempfile
import os
import json
import time
import shutil
from datetime import datetime

app = Flask(__name__)

# Configuration - these will be overridden by request parameters
DEFAULT_ENHSP_PATH = "/home/shermin/ENHSP-Public/enhsp.jar"  # Default path to ENHSP JAR file
DEFAULT_DOMAIN_FILE_PATH = "Plannerinputs/domain.pddl"  # Default path to domain file
DEFAULT_PROBLEM_FILE_PATH = "Plannerinputs/problem.pddl"  # Default path to problem file
DEFAULT_TIMEOUT_SECONDS = 120

@app.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({
        "status": "healthy",
        "timestamp": datetime.now().isoformat(),
        "enhsp_path": DEFAULT_ENHSP_PATH,
        "enhsp_available": os.path.exists(DEFAULT_ENHSP_PATH),
        "domain_file_available": os.path.exists(DEFAULT_DOMAIN_FILE_PATH),
        "problem_file_available": os.path.exists(DEFAULT_PROBLEM_FILE_PATH)
    })

@app.route('/plan', methods=['POST'])
def create_plan():
    """Main planning endpoint"""
    try:
        # Parse request
        data = request.json
        print(f"Received planning request: {json.dumps(data, indent=2)}")
        
        # Extract data - handle both old and new request formats
        planning_type = data.get('planningType', 'PDDL')
        
        # Extract PDDL-specific properties (new format)
        domain_file_path = data.get('domainFile', DEFAULT_DOMAIN_FILE_PATH)
        problem_file_path = data.get('problemFile', DEFAULT_PROBLEM_FILE_PATH)
        planner_path = data.get('plannerPath', DEFAULT_ENHSP_PATH)
        timeout_seconds = data.get('timeoutSeconds', DEFAULT_TIMEOUT_SECONDS)
        max_plan_length = data.get('maxPlanLength', 20)
        
        # Extract legacy properties (old format) for backward compatibility
        available_actions = data.get('availableActions', [])
        initial_state = data.get('initialState', {})
        goals = data.get('goals', [])
        planner_config = data.get('plannerConfig', {})
        
        # Use planner_config values if not specified in new format
        if planner_config:
            if not domain_file_path or domain_file_path == DEFAULT_DOMAIN_FILE_PATH:
                domain_file_path = planner_config.get('domainFile', domain_file_path)
            if not problem_file_path or problem_file_path == DEFAULT_PROBLEM_FILE_PATH:
                problem_file_path = planner_config.get('problemFile', problem_file_path)
            if not planner_path or planner_path == DEFAULT_ENHSP_PATH:
                planner_path = planner_config.get('plannerPath', planner_path)
            if timeout_seconds == DEFAULT_TIMEOUT_SECONDS:
                timeout_seconds = planner_config.get('timeoutSeconds', timeout_seconds)
        
        # Log extracted properties
        print(f"Extracted PDDL properties:")
        print(f"  - Domain file: {domain_file_path}")
        print(f"  - Problem file: {problem_file_path}")
        print(f"  - Planner path: {planner_path}")
        print(f"  - Timeout: {timeout_seconds} seconds")
        print(f"  - Max plan length: {max_plan_length}")
        
        if planning_type != 'PDDL':
            return jsonify({
                'success': False,
                'error': {
                    'code': 'UNSUPPORTED_PLANNING_TYPE',
                    'message': f'Planning type {planning_type} not supported',
                    'details': 'Only PDDL planning is currently supported'
                }
            }), 400
        
        # Check if ENHSP is available
        if not os.path.exists(planner_path):
            return jsonify({
                'success': False,
                'error': {
                    'code': 'ENHSP_NOT_FOUND',
                    'message': 'ENHSP planner not found',
                    'details': f'ENHSP not found at {planner_path}'
                }
            }), 500
        
        # Check if domain and problem files exist
        if not os.path.exists(domain_file_path):
            return jsonify({
                'success': False,
                'error': {
                    'code': 'DOMAIN_FILE_NOT_FOUND',
                    'message': 'Domain file not found',
                    'details': f'Domain file not found at {domain_file_path}'
                }
            }), 500
            
        if not os.path.exists(problem_file_path):
            return jsonify({
                'success': False,
                'error': {
                    'code': 'PROBLEM_FILE_NOT_FOUND',
                    'message': 'Problem file not found',
                    'details': f'Problem file not found at {problem_file_path}'
                }
            }), 500
        
        # Copy existing domain and problem files to temporary location
        domain_file = copy_file_to_temp(domain_file_path, 'domain_')
        problem_file = copy_file_to_temp(problem_file_path, 'problem_')
        
        # Call ENHSP
        start_time = time.time()
        plan_result = call_enhsp(domain_file, problem_file, planner_path, timeout_seconds)
        planning_time = time.time() - start_time
        
        # Clean up temporary files
        os.unlink(domain_file)
        os.unlink(problem_file)
        
        if plan_result['success']:
            # Convert ENHSP output to plan string format
            plan_string = convert_enhsp_to_plan_string(plan_result['plan'])
            
            return jsonify({
                'success': True,
                'plan': plan_string,
                'planningTimeSeconds': planning_time,
                'planLength': len(plan_result['plan']),
                'plannerUsed': 'ENHSP'
            })
        else:
            return jsonify({
                'success': False,
                'error': f'ENHSP failed to find a plan: {plan_result["error"]}',
                'planningTimeSeconds': planning_time,
                'plannerUsed': 'ENHSP'
            })
            
    except Exception as e:
        print(f"Error in planning request: {str(e)}")
        return jsonify({
            'success': False,
            'error': f'Unexpected error during planning: {str(e)}'
        }), 500

def copy_file_to_temp(source_path, prefix):
    """Copy a file to a temporary location"""
    fd, temp_filename = tempfile.mkstemp(suffix='.pddl', prefix=prefix)
    with os.fdopen(fd, 'w') as temp_file:
        with open(source_path, 'r') as source_file:
            temp_file.write(source_file.read())
    
    print(f"Copied {source_path} to temporary file: {temp_filename}")
    return temp_filename

def call_enhsp(domain_file, problem_file, planner_path, timeout_seconds):
    """Call ENHSP planner"""
    try:
        # Build ENHSP command (Java application)
        cmd = [
            'java', '-jar', planner_path,
            '-o', domain_file,
            '-f', problem_file,
            '-planner', 'pt-blind'  # Use pt-blind configuration
        ]
        
        print(f"Calling ENHSP with command: {' '.join(cmd)}")
        
        # Run ENHSP
        result = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            timeout=timeout_seconds
        )
        
        print(f"ENHSP stdout: {result.stdout}")
        print(f"ENHSP stderr: {result.stderr}")
        
        if result.returncode == 0:
            # Parse ENHSP output
            plan = parse_enhsp_output(result.stdout)
            return {'success': True, 'plan': plan}
        else:
            return {
                'success': False, 
                'error': f'ENHSP failed with return code {result.returncode}: {result.stderr}'
            }
            
    except subprocess.TimeoutExpired:
        return {'success': False, 'error': 'ENHSP planning timed out'}
    except Exception as e:
        return {'success': False, 'error': f'Error calling ENHSP: {str(e)}'}

def parse_enhsp_output(output):
    """Parse ENHSP output to extract plan"""
    plan = []
    lines = output.split('\n')
    
    for line in lines:
        line = line.strip()
        # Look for lines with timestamp and action like "0.0: (grab lp1 fp1 r1)"
        if ':' in line and '(' in line and line.endswith(')'):
            # Extract the part after the colon and before the parentheses
            colon_index = line.find(':')
            if colon_index != -1:
                action_part = line[colon_index + 1:].strip()
                if action_part.startswith('(') and action_part.endswith(')'):
                    # Parse action like "(grab lp1 fp1 r1)"
                    action_str = action_part[1:-1]  # Remove parentheses
                    parts = action_str.split()
                    
                    if len(parts) >= 2:
                        action_name = parts[0]
                        parameters = parts[1:]
                        
                        plan.append({
                            'name': action_name,
                            'parameters': parameters
                        })
    
    return plan

def convert_enhsp_to_plan_string(enhsp_plan):
    """Convert ENHSP plan to plan string format (like NodeGraphGenerated.txt)"""
    plan_lines = []
    
    # Add action instances
    for i, action in enumerate(enhsp_plan):
        # Convert ENHSP action to action string format
        action_name = action['name'].title()  # Capitalize first letter
        parameters = action['parameters']
        
        # Create action string like "Grab_b1_fp2_r1"
        action_string = f"{action_name}_{'_'.join(parameters)}"
        plan_lines.append(f"ActionInstance: {action_string}")
    
    # Add sequential relations
    for i in range(len(enhsp_plan) - 1):
        action1_name = enhsp_plan[i]['name'].title()
        action2_name = enhsp_plan[i + 1]['name'].title()
        plan_lines.append(f"Relation: {action1_name} MEETS {action2_name}")
    
    return '\n'.join(plan_lines)

if __name__ == '__main__':
    print("Starting PDDL Planning Service...")
    print(f"Default ENHSP path: {DEFAULT_ENHSP_PATH}")
    print(f"Default ENHSP available: {os.path.exists(DEFAULT_ENHSP_PATH)}")
    print(f"Default domain file path: {DEFAULT_DOMAIN_FILE_PATH}")
    print(f"Default domain file available: {os.path.exists(DEFAULT_DOMAIN_FILE_PATH)}")
    print(f"Default problem file path: {DEFAULT_PROBLEM_FILE_PATH}")
    print(f"Default problem file available: {os.path.exists(DEFAULT_PROBLEM_FILE_PATH)}")
    
    app.run(host='0.0.0.0', port=5000, debug=True)
