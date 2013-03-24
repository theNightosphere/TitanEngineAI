//---------------------------------------------------------------------------------------------------------------------
//  Name:           AIComponent (Titan.AI.AIComponent)
//  Description:    The component that ties an entity to the AI engine. When an entity adds an AI component, it is 
//                  registered with the AI engine and scheduled to update during the engine's update ticks.
//
//  Copyright (c) 2011-2013 Game Design & Development at UWM, All Rights Reserved
//---------------------------------------------------------------------------------------------------------------------
//  NOTES:
//  TODO:
//---------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

using Titan.World;

namespace Titan.AI
{
    
    class AIComponent
    {
        //-------------------------------------------------------------------------------------------------------------
        // Public
        //-------------------------------------------------------------------------------------------------------------
        public AIComponent(GameObject parentObject, string behaviorTreeHandle, AIManager aiManager)
        {
            m_ParentObject = parentObject;
            m_BehaviorTree = aiManager.GetTree(behaviorTreeHandle);


        }

        //-------------------------------------------------------------------------------------------------------------
        // Private
        //-------------------------------------------------------------------------------------------------------------

        private Boolean RegisterObject(GameObject parentObject, string BehaviorTreeHandle, AIManager aiManager)
        {
            AIActor newActor = new AIActor();
            if (newActor.Initialize(aiManager.NextNewActorID, BehaviorTreeHandle, parentObject))
            {
                if (aiManager.RegisterActor(newActor))
                {
                    // Actor was successfully created and registered, increment global id for actors.
                    aiManager.NextNewActorID++;
                }
            }
            return true;
        }

        GameObject m_ParentObject;
        BehaviorTree m_BehaviorTree;
    }

    
}
