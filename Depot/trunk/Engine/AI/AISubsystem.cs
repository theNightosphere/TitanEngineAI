//---------------------------------------------------------------------------------------------------------------------
//  Name:           AI Subsystem [Titan.AI.AISubsystem]
//  Description:    The subsystem responsible for determining how often to update registered entities along with 
//                  initializing the interpreters, sorting and sending deferred action requests, and handling the 
//                  results of those actions.          
//
//  Copyright (c) 2011-2013 Game Design & Development at UWM, All Rights Reserved
//---------------------------------------------------------------------------------------------------------------------
//  NOTES:
//
//  TODO: - Create list of in-view actors. (Currently done, but never initialized. Requires method to poll WorldTree)
//        - Determine when to update actor's private blackboards.
//        - Create a Pair of AIActor and Deferred action? This will allow deferred action results to be sorted by 
//          actor and possibly solve problem of how to call 
//        - Use Titan.World.Tree.WorldTree.GetObjectsInRange(int leftBound, int rightBound) to get a 
//          LinkedList<GameObject>. Check for GameObjects with components.
//     .
//---------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Titan.World;

namespace Titan.AI
{
    using DeferredAction = Titan.AI.ShapeToken.LeafActionDelegate;

    class AISubsystem
    {

        //-------------------------------------------------------------------------------------------------------------
        // Public
        //-------------------------------------------------------------------------------------------------------------

        /// Update
        /// 
        /// <summary>
        /// The update tick for the AI subsystem. On tick 1 (starting from tick 0), the list of in-range actors is
        /// updated. On tick 3, all in-range actors traverse their respective behavior trees.
        /// </summary>
        /// <param name="deltaTime">The time elapsed between the current and previous update tick.</param>
        public void Update(float deltaTime, Camera currentCamera, WorldTree worldTree)
        {
            
            if (m_NumOfUpdateTicks == 1)
            {
                // Query for list of in-range actors.
                LinkedList<GameObject> listOfInRangeObjects = worldTree.GetObjectsInRange(currentCamera.Left, currentCamera.Right);
                m_InRangeActors.Clear();
                foreach (GameObject g in listOfInRangeObjects)
                {
                    if (g.GetComponent(typeof(AIComponent)) != null)
                    {
                        // Needs to be an actual AIActor.
                        m_InRangeActors.Add(new AIActor());
                    }
                }
                
            }
            else if (m_NumOfUpdateTicks == 3)
            {
                //Spawn Interpreters, traverse trees.
                TreeInterpreter runningInterpreter = new TreeInterpreter();
                runningInterpreter.Initialize(m_InRangeActors);
                runningInterpreter.UpdateAll(deltaTime);

                Queue<DeferredAction> deferredActions = runningInterpreter.DeferredActionQueue;

                // Loading of deferredActions and subsequent execution is split so that loading
                // can be done on multiple threads and actions are executed on a single thread once all interpreters
                // have updated.
                while (deferredActions.Count > 0)
                {
                    m_ActionsToExecute.Enqueue(deferredActions.Dequeue());
                }

            }


            //If, after incrementing, number of ticks is 4, then set it to zero. Otherwise, make no change.
            m_NumOfUpdateTicks = (++m_NumOfUpdateTicks == 4) ? 0 : m_NumOfUpdateTicks;
        }

        /// Constructor
        /// 
        /// <summary>
        /// Initializes the AI Subsystem.
        /// </summary>
        public AISubsystem()
        {
            m_NumOfUpdateTicks = 0;
            m_InRangeActors = new List<AIActor>();
            m_ActionsToExecute = new Queue<DeferredAction>();
        }

        //-------------------------------------------------------------------------------------------------------------
        // Private
        //-------------------------------------------------------------------------------------------------------------

        int                   m_NumOfUpdateTicks;
        List<AIActor>         m_InRangeActors;
        Queue<DeferredAction> m_ActionsToExecute;
        const UInt32          FRAME_OFFSET_FROM_CAMERA = 100;
    }
}
