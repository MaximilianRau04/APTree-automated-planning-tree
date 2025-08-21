To set up the python planning service, the following steps are required:
1.  Activate the virtual environment: Open your WSL terminal and navigate to the python_service directory, then activate the virtual environment:

###commands:
cd /mnt/c/Users/sherk/Documents/BehaviorTreeMainProject/BehaviorTreeMainProject/python_service
source pddl_env/bin/activate

2. Start PDDL Planning Service:

###Commands:
python pddl_planning_service.py


Make sure to have dependencies installed: 
###Commands:
pip install flask requests






