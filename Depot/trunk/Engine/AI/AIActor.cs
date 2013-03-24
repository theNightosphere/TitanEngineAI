//---------------------------------------------------------------------------------------------------------------------
//  Name:           AI Actor [Titan.AI.AIActor]
//  Description:    Object that stores the data representing its most recent traversal of the actor's behavior tree.                  
//
//  Copyright (c) 2011-2013 Game Design & Development at UWM, All Rights Reserved
//---------------------------------------------------------------------------------------------------------------------
//  NOTES: AIActor's m_GameObject is a reference to Titan.World.GameObject, which is an Abstract class. When this
//         class is extended, I should check if it makes sense to instantiate as one of the children classes.
//
//  TODO:
//     .
//---------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Titan.AI
{
    class AIActor
    {
        //-------------------------------------------------------------------------------------------------------------
        // Public
        //-------------------------------------------------------------------------------------------------------------


        /// Initialize
        /// 
        /// <summary>
        /// Initializes an instance of the AIActor. It is assumed the actor has made no previous traversals.
        /// </summary>
        /// <param name="ActorID">An unsigned integer corresponding to the actor's game object's uid.</param>
        /// <param name="treeHandle">A string corresponding to the actor's behavior tree.</param>
        /// <param name="actorsGameObject">This actor's GameObject. A reference to the actor's other components.</param>
        /// <returns>True if initialization succeeds, False if one or more fields are not properly initialized.</returns>
        public bool Initialize(uint ActorID, string treeHandle, Titan.World.GameObject actorsGameObject)
        {
            m_ActorHandle = ActorID;
            if( m_ActorHandle == uint.MaxValue ){ return false; }
            
            m_TreeHandle = treeHandle;
            if( m_TreeHandle == null){ return false; }

            /*m_ActorsGameObject = actorsGameObject;
            if (m_ActorsGameObject == null) { return false; }*/

            return true;
        }

        /// Initialize
        /// 
        /// <summary>
        /// Initializes an instance of the AIActor. 
        /// </summary>
        /// <param name="ActorID">An unsigned integer corresponding to the actor's game object's uid.</param>
        /// <param name="treeHandle">A string corresponding to the actor's behavior tree.</param>
        /// <param name="previousTraversal">A stack of traversal tokens from the previous traversal.</param>
        /// <param name="deferredActionResults">A list of traversal tokens with the return value from the deferred 
        /// actions emitted during previous traversals.</param>
        /// <param name="actorsGameObject">This actor's GameObject. A reference to the actor's other components.</param>
        /// <returns>True if initialization succeeds, False if one or more fields are not properly initialized.</returns>
        public bool Initialize(uint ActorID, string treeHandle, Stack<TraversalToken> previousTraversal,
            List<TraversalToken> deferredActionResults, Titan.World.GameObject actorsGameObject)
        {
            m_ActorHandle = ActorID;
            if( m_ActorHandle == uint.MaxValue ){ return false; }
            
            m_TreeHandle = treeHandle;
            if( m_TreeHandle == null){ return false; }

            m_PreviousTraversalStack = previousTraversal;
            if( m_PreviousTraversalStack == null){ return false; }

            m_DeferredActionResults = deferredActionResults;
            if( m_DeferredActionResults == null){ return false; }

            m_ActorsGameObject = actorsGameObject;
            if (m_ActorsGameObject == null) { return false; }

            return true;
        }

        /// Constructor
        /// 
        /// <summary>
        /// Instantiates an empty AI Actor. This class is not usable until Initialize has been called to provide a 
        /// handle to the game object and its tree.
        /// </summary>
        public AIActor()
        {
            m_ActorHandle = UInt32.MaxValue;
            m_TreeHandle = null;
            m_PreviousTraversalStack = new Stack<TraversalToken>();
            m_DeferredActionResults = new List<TraversalToken>();
            // TODO: Replace with a non-abstract GameObject?
            m_ActorsGameObject = null;
        }

        public Stack<TraversalToken> PreviousTraversalStack
        {
            get 
            { 
                return m_PreviousTraversalStack; 
            }
            set
            {
                if (value != null) { m_PreviousTraversalStack = value; }
            }

        }

        public List<TraversalToken> DeferredActionResults
        {
            get
            {
                return m_DeferredActionResults;
            }
            set
            {
                if (value != null) { m_DeferredActionResults = value; }
            }
        }

        public string TreeHandle
        {
            get
            {
                return m_TreeHandle;
            }
        }

        public Titan.World.GameObject ActorsGameObject
        {
            get
            {
                return m_ActorsGameObject;
            }
            private set{ m_ActorsGameObject = value; }
        }

        //-------------------------------------------------------------------------------------------------------------
        // Private
        //-------------------------------------------------------------------------------------------------------------

        uint                      m_ActorHandle;
        Titan.World.GameObject    m_ActorsGameObject;
        string                    m_TreeHandle;
        Stack<TraversalToken>     m_PreviousTraversalStack;
        // The list of return values for TraversalTokens that performed an action which could not be checked for 
        // success or failure within a single update tick.
        List<TraversalToken>      m_DeferredActionResults;  
    }
}
