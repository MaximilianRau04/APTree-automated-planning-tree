# Navigate to your project folder
cd /mnt/c/Users/sherk/Documents/BehaviorTreeMainProject/BehaviorTreeMainProject/python_service

    source pddl_env/bin/activate

       python pddl_planning_service.py



# start the planutils docker image


#fixing docker deamon
sudo systemctl status docker
sudo systemctl start docker
sudo dockerd &

# start the docker
docker start  stupefied_hellman

planutils activate