//---------------------------------------------------------------------------------------------------------------------
//  Name:           AI Manager [Titan.AI.AIManager]
//  Description:                      
//
//  Copyright (c) 2011-2013 Game Design & Development at UWM, All Rights Reserved
//---------------------------------------------------------------------------------------------------------------------
//  NOTES:
//
//  TODO:
//     .
//---------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Titan.AI
{
    class AIManager
    {

        //-------------------------------------------------------------------------------------------------------------
        // Public
        //-------------------------------------------------------------------------------------------------------------

        public bool LoadTrees()
        {
            // Does nothing yet. In the future, will take a list of files, initialize the behavior trees described,
            // and add them to a master dictionary of behavior trees.
            return true;
        }

        /// <summary>
        /// Accesses the BehaviorTree with the handle desiredTreeHandle. This method does not check whether the entry
        /// is an existing BehaviorTree. The calling context must ensure that the BehaviorTree exists before calling
        /// or risk an error.
        /// </summary>
        /// <param name="desiredTreeHandle">The string handle of the BehaviorTree to be accessed.</param>
        /// <returns>The BehaviorTree with the handle desiredTreeHandle.</returns>
        public BehaviorTree GetTree(string desiredTreeHandle)
        {
            return m_ActiveTrees[desiredTreeHandle];
        }

        /// <summary>
        /// Checks whether the AIManager has a BehaviorTree that corresponds to the string handle passed as a
        /// parameter.
        /// </summary>
        /// <param name="handleOfTreeToCheck">The string handle of the BehaviorTree being checked for existence.
        /// </param>
        /// <returns>True if the AIManager has a BehaviorTree corresponding to the string handle, false if
        /// it does not have such a BehaviorTree.</returns>
        public bool IsActiveTree(string handleOfTreeToCheck)
        {
            return m_ActiveTrees.ContainsKey(handleOfTreeToCheck);
        }

        /// RegisterActor
        /// <summary>
        /// Takes an AIActor and adds it to the list of AIActors representing GameObjects with an AIComponent.
        /// Nothing is added if the AIActor is already registered.
        /// </summary>
        /// <param name="actorToRegister">The AIActor to add to the list of registered actors.</param>
        /// <returns>True if actor is added successfully, false if the actor is not added.</returns>
        public bool RegisterActor(AIActor actorToRegister)
        {
            if(!m_RegisteredActors.Contains(actorToRegister))
            {
                m_RegisteredActors.Add(actorToRegister);
                return true;
            }

            return false;
        }

        /// Constructor
        /// 
        /// <summary>
        /// Instantiates a basic instance of the AI Manager. Not usable until it is initialized.
        /// </summary>
        public AIManager()
        {
            m_ActiveTrees = new Dictionary<string, BehaviorTree>();
            m_RegisteredActors = new List<AIActor>();
        }

        public UInt32 NextNewActorID
        {
            get
            {
                return m_NextNewActorID;
            }
            set
            {
                m_NextNewActorID = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // Private
        //-------------------------------------------------------------------------------------------------------------

        private static UInt32 m_NextNewActorID = 1;

        Dictionary<string, BehaviorTree> m_ActiveTrees;
        List<AIActor> m_RegisteredActors; // A list of AIActors associated with GameObjects that have an AIComponent.

    }
}
