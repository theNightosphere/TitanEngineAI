//---------------------------------------------------------------------------------------------------------------------
//  Name:           Shape Token [Titan.AI.ShapeToken]
//  Description:    A token that represents a node within a behavior tree. It is used to guide traversal, but does not
//                  store any instance data specific to an actor's tree traversal.                  
//
//  Copyright (c) 2011-2013 Game Design & Development at UWM, All Rights Reserved
//---------------------------------------------------------------------------------------------------------------------
//  NOTES:
//
//  TODO:  -Determine how to store the functions that can be accessed by a shape token. 
//         -Consider whether or not to add boolean value to determine if leaf node's action is immediate or deferred.
//         -Initialize 'isLeafActionImmediate' at some point.
//     .
//---------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Titan.AI
{
    class ShapeToken
    {
        //-------------------------------------------------------------------------------------------------------------
        // Public
        //-------------------------------------------------------------------------------------------------------------

        /// Initialize
        /// <summary>
        /// Initializes an instantiated Shape Token. A Shape Token must be initialized before its first use.
        /// </summary>
        /// <param name="tokenHandle">The handle which is used to identify this token. Used mostly for logging
        /// purposes</param>
        /// <param name="tokenType">An enumeration that determines a token's type (Selector, Sequence, Decorator, Leaf)
        /// which is used to determine traversal semantics.</param>
        /// <param name="childrenIndices">A list of integers corresponding to the indices of this token's children
        /// within the associated behavior tree.</param>
        /// <param name="positionInTree">The integer index of this token's location within its associated behavior
        /// tree.</param>
        /// <param name="endOfRange">The integer index of the last child token within this token's subtree.</param>
        /// <returns>True if initialization succeeds, False if at least one property was not properly
        /// initialized.</returns>
        public bool Initialize(string tokenHandle, TokenType tokenType, List<int> childrenIndices, int positionInTree,
            int endOfRange, LeafActionDelegate leafAction)
        {
            m_Handle = tokenHandle;
            if (m_Handle == null)
            {
                return false;
            }

            m_TokenType = tokenType;
            if (tokenType == TokenType.NoType)
            {
                return false;
            }

            m_ChildrenIndices = childrenIndices;
            if ((m_TokenType == TokenType.Leaf && m_ChildrenIndices.Count > 0) ||
                (m_TokenType != TokenType.Leaf && m_ChildrenIndices.Count < 1))
            {
                return false;
            }

            m_PositionInTree = positionInTree;
            if (m_PositionInTree < 0) // A token's position must be at least 0, aka the root.
            {
                return false;
            }

            m_EndOfRange = endOfRange;
            // The end of a token's range cannot be earlier than its own position.
            if (m_EndOfRange < 0 || m_EndOfRange < m_PositionInTree)
            {
                return false;
            }

            if (m_TokenType == TokenType.Leaf)
            {
                m_LeafActionDelegate = leafAction;

                if (m_LeafActionDelegate == null)
                {
                    return false;
                }
            }
            else
            {
                m_LeafActionDelegate = null;
            }

            return true;
        }

        public static bool operator ==(ShapeToken a, ShapeToken b)
        {
            return (a.Handle == b.Handle) && (a.CurrentTokenType == b.CurrentTokenType);
        }
        public static bool operator !=(ShapeToken a, ShapeToken b)
        {
            return !(a == b);
        }

        /// Constructor
        /// <summary>
        /// Instantiates a trivial instance of the ShapeToken class with dummy values. Not usable until it has been
        /// initialized.
        /// </summary>
        public ShapeToken()
        {
            m_Handle = null;
            m_TokenType = TokenType.NoType;
            m_ChildrenIndices = new List<int>();
            m_PositionInTree = -1;
            m_EndOfRange = -1;
            m_IsLeafActionImmediate = false;
        }

        public enum TokenType
        {
            Selector,
            Sequence,
            Decorator,
            Leaf,
            NoType
        }

        public string Handle
        {
            get
            {
                return m_Handle;
            }
        }

        public TokenType CurrentTokenType
        {
            get
            {
                return m_TokenType;
            }
        }

        public List<int> ChildrenIndices
        {
            get
            {
                return m_ChildrenIndices;
            }
        }

        public int PositionInTree
        {
            get
            {
                return m_PositionInTree;
            }
        }

        public int EndOfRange
        {
            get
            {
                return m_EndOfRange;
            }
        }

        public LeafActionDelegate LeafAction
        {
            get
            {
                return m_LeafActionDelegate;
            }
        }

        public bool IsLeafActionImmediate
        {
            get
            {
                return m_IsLeafActionImmediate;
            }
            set
            {
                m_IsLeafActionImmediate = value;
            }
        }

        // The action performed by leaf nodes. 
        public delegate TraversalToken.TokenState LeafActionDelegate(Titan.World.GameObject ActionPerformingEntity);

        //-------------------------------------------------------------------------------------------------------------
        // Private
        //-------------------------------------------------------------------------------------------------------------

        string                m_Handle;
        TokenType             m_TokenType;
        List<int>             m_ChildrenIndices;
        int                   m_PositionInTree;
        int                   m_EndOfRange; //The index of the last node in this node's subtree
        LeafActionDelegate    m_LeafActionDelegate;
        bool                  m_IsLeafActionImmediate;
    }

}
