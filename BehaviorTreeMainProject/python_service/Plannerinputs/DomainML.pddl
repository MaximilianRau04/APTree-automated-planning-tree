(define (domain fit) 
(:requirements 
  :adl
)

  (:types 
    
    equipposition firstposition positiononrail stackposition - location      
                                                     
    vaccumgripper nailgripper gluegun - tool 
                                                
    plate beam - element

    stack -layer
    cassette - module 
    robot - agent                                 
  )
 
  (:predicates
    (atagent ?client - robot ?pp - location)   ;robot is at position pp
    (atplace ?obj - element ?p - location) ; object is at position p
    (attool ?tool - tool ?ep - equipposition)  ;tool is at an equip position
    (hastool ?client - robot ?tool - tool)    ;a robot is equipped with a tool                                      
    (robotequipped ?client - robot)      ;a robot is not wquipped with a tool                              
    (activetool ?tool - tool)          ; a tool is active                       
    (holding ?client - robot ?obj - element)         ; a robot is holding an object
    (clear ?obj - element)     ;an object is clear
    (ontop ?obj1 - element ?obj2 - element); object one is on top of object 2
    (vgempty ?client - robot); vaccum gripper is empty (not holding any object)
    (glued ?obj - element); an object is glued
    (nailed ?obj - element)   ;an object is nailed
    (positionfree ?pos - location)
      
)

 ;robot travels from one location to another
    (:action travelML
    :parameters (?client - robot ?from  - location   ?to - location)
   
    :precondition (and
      (pos ?client ?from)                                   
      (not (= ?from ?to))                                  
    )
    
    :effect (and
    (not (pos ?client ?from))                               
    (pos ?client ?to)                                      
        )
    
    )
  ;robot equips the endeffector
    (:action equipeML
    :parameters (?client - robot ?too - tool ?ep - equipposition)
    
    :precondition (and
      (attool ?too ?ep)                                    
      (empty ?client)                                       
      (pos ?client ?ep)   
      (not(positionfree ?ep))                                  
    )   
    :effect (and
    
      (have ?client ?too)
      (not (empty ?client))
      (not (attool ?too ?ep))
      (positionfree ?ep)
      
      
    )
    )
    ;robot puts the endeffector down
     (:action deequipML
      :parameters (?client - robot ?too - tool ?ep - equipposition) 
      :precondition (and
      (pos ?client ?ep)
      (have ?client ?too)
      (not (active ?too))
      (not (attool ?too ?ep))
      (not (empty ?client))
      (positionfree ?ep)
      
                     )
      
      :effect  (and 
      (attool ?too ?ep) 
      (empty ?client)    
      (not (have ?client ?too))
      (not (positionfree ?ep))
      
      )
      )       
  
 
    ;turns on the tool (end-effector)
    (:action initializeML
    :parameters (?client - robot ?too - tool)
  
    :precondition (and
      
      (not (empty ?client)) 
      (have ?client ?too)
      (not (active ?too)) 
    )
    :effect 
    
      (active ?too)  
          
    )
     ;turns of the tool
     (:action closetoolML
      :parameters (?client - robot ?too - tool ?obj - element)
      :precondition (and 
      (active ?too)
      (vgempty ?client) 
      
     )
      :effect  
      (not (active ?too))      
  
    )
    ;robot grabs an object from the table
    (:action pickUpML
    :parameters (?obj - element ?p - location  ?client - robot ?vg - vaccumgripper)
    
    :precondition (and    
      (have ?client ?vg)               
      (active ?vg)                      
      (atplace ?obj ?p)
      (pos ?client ?p)     
      (vgempty ?client)             
      (not (holding ?client ?obj))  
      (not (positionfree ?p)) 
      (clear ?obj)    
      (not(glued ?obj))
      (not (nailed ?obj))
                  )
     
    :effect  (   and
                 (holding ?client ?obj)
                 (not(atplace ?obj ?p))
                 (not(vgempty ?client))
                 (not(clear ?obj))
                 (positionfree ?p)
             )
    )
    ;robot places an object on the table
     (:action placeML
    :parameters (?obj - element ?p -  location   ?client - robot ?vg -vaccumgripper)
    
    :precondition (and
      (not(vgempty ?client))
      (holding ?client ?obj)  
      (pos ?client ?p)     
      (active ?vg)  
      (not(clear ?obj)) 
      (positionfree ?p)     
                  )
     
    :effect (and
      (atplace ?obj ?p)       
      (not (holding ?client ?obj))
      (vgempty ?client) 
      (clear ?obj)
      (not(positionfree ?p))
            )        
  )
     

     (:action stackML ; for placing object 1 on object 2 based on their capacity
    :parameters (?obj1 - element ?obj2 - element  ?client - robot ?vg - vaccumgripper ?pr - positiononrail ?lay - layer ?mod - module)
    
    :precondition (and
      (not(vgempty ?client))
      (holding ?client ?obj1)  
      (pos ?client ?pr)     
      (active ?vg)  
      (clear ?obj2) 
      (atplace ?obj2 ?pr)    
      (not (atplace ?obj1 ?pr)) 
                  )
     
    :effect (and  
             
      (ontop ?obj1 ?obj2)       
      (not (holding ?client ?obj1))     
      (atplace ?obj1 ?pr)   
      (vgempty ?client)  
      (not (clear ?obj2))  
      (clear ?obj1)
            )
    
    )

     
    (:action stackonmultipleML
    :parameters (?plate - plate ?client - robot ?p -positiononrail ?vg - vaccumgripper ?mod - module ?lay -layer)
    :precondition (and
        (holding ?client ?plate)
        (pos ?client ?p)
        (active ?vg)
        (not (atplace ?plate ?p))      
        
    )
    
    :effect (and
                       
        (atplace ?plate ?p)  
                           
        )
 
    )



     (:action gluingML
        :parameters (?obj - element ?p - positiononrail ?client - robot ?gg -gluegun)
        :precondition (and 
    
        (pos ?client ?p)
        (atplace ?obj ?p) 
        (clear ?obj)      
        (active ?gg)
        (not (glued ?obj))
        )

        :effect  
        (glued ?obj)
               
    )
    

      (:action nailingML
        :parameters (?obj - element ?p - positiononrail ?client - robot ?ng -nailgripper)
        :precondition (and 
        (pos ?client ?p)
        (atplace ?obj ?p)
        (clear ?obj)
        (active ?ng)
        (not (nailed ?obj))
        )

        :effect  
        (nailed ?obj)              
    )
)
     
  
  
   
     
  
  
  

  