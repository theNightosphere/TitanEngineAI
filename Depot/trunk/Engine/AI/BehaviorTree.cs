//---------------------------------------------------------------------------------------------------------------------
//  Name:           Behavior Tree [Titan.AI.BehaviorTree]
//  Description:    A flattened representation of a Behavior Tree (which is actually a DAG) that contains all of its 
//                  nodes and usable functions.                
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
    class BehaviorTree
    {
        //-------------------------------------------------------------------------------------------------------------
        // Public
        //-------------------------------------------------------------------------------------------------------------

        /// Initialize
        /// 
        /// <summary>
        /// Initializes a behavior tree. 
        /// </summary>
        /// <param name="treeHandle">The string handle that is used to reference the tree. Tree handles should be
        /// unique, otherwise errors may occur.</param>
        /// <param name="treeNodes">A list of shape tokens that is ordered according to a depth-first traversal
        /// of the behavior tree.</param>
        /// <returns>True if the behavior tree is initialized properly, False if any of the fields contains an invalid
        /// value.</returns>
        public bool Initialize(string treeHandle, List<ShapeToken> treeNodes)
        {
            m_Handle = treeHandle;
            if (m_Handle == null) { return false; }

            m_Nodes = treeNodes;
            //A valid tree will contain at least one node. 
            if (m_Nodes.Count < 1) { return false; }

            return true;
        }

        /// Constructor
        /// <summary>
        /// Instantiates a trivial instance of the Behavior Tree. Cannot be used until it has been initialized by a 
        /// call to Initialize.
        /// </summary>
        public BehaviorTree()
        {
            m_Handle = null;
            m_Nodes = new List<ShapeToken>();
        }

        public string Handle { get { return m_Handle; } }

        public List<ShapeToken> Nodes { get { return m_Nodes; } }

        //-------------------------------------------------------------------------------------------------------------
        // Private
        //-------------------------------------------------------------------------------------------------------------

        string           m_Handle;
        List<ShapeToken> m_Nodes;

    }
}
