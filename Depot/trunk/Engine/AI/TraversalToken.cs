//---------------------------------------------------------------------------------------------------------------------
//  Name:           Traversal Token [Titan.AI.TraversalToken]
//  Description:    A token that represents a node within a behavior tree. It is used to guide traversal, and stores 
//                  the instance data of an actor's behavior tree traversal.
//
//  Copyright (c) 2011-2013 Game Design & Development at UWM, All Rights Reserved
//---------------------------------------------------------------------------------------------------------------------
//  NOTES:   The Traversal Token is different from the Shape Token in that it is meant to be short lived and modified
//           often, versus the persistent and almost read-only nature of the Shape Token.
//
//  TODO:   -Instantiate list of previously run children.
//     .
//---------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Titan.AI
{

    class TraversalToken
    {

        //-------------------------------------------------------------------------------------------------------------
        // Public
        //-------------------------------------------------------------------------------------------------------------

        /// Initialize
        /// <summary>
        /// Initializes an instantiated Traversal Token. A Traversal Token must be initialized before its first use.
        /// </summary>
        /// <param name="referenceToken">The ShapeToken that is used as the base for this TraversalToken.</param>
        /// <param name="currentState">An enumeration representing the state of the token based on the previous
        /// traversal.</param>
        /// <returns>True if the initialization succeeds, False if any property is not properly initialized.</returns>
        public bool Initialize(ShapeToken referenceToken, TokenState currentState)
        {
            m_ReferenceToken = referenceToken;
            //If the reference token has not been properly initialized, this traversal token cannot be initialized.
            if (m_ReferenceToken.CurrentTokenType == ShapeToken.TokenType.NoType)
            {
                return false;
            }

            m_CurrentState = currentState;
            if (m_CurrentState == TokenState.Uninitialized)
            {
                return false;
            }

            return true;
        }

        /// Initialize
        /// <summary>
        /// Initializes an instantiated Traversal Token. A Traversal Token must be initialized before its first use.
        /// The Token is set to the Ready state by default.
        /// </summary>
        /// <param name="referenceToken">The ShapeToken that is used as the base for this TraversalToken.</param>
        /// <returns>True if the initialization succeeds, False if any property is not properly initialized.</returns>
        public bool Initialize(ShapeToken referenceToken)
        {
            m_ReferenceToken = referenceToken;
            //If the reference token has not been properly initialized, this traversal token cannot be initialized.
            if (m_ReferenceToken.CurrentTokenType == ShapeToken.TokenType.NoType)
            {
                return false;
            }

            m_CurrentState = TokenState.Ready;

            m_CurrentRunningChild = 0;

            return true;
        }

        /// GetNextChild
        /// 
        /// <summary>
        /// Returns the next child to be considered in a tree traversal. What child is called next is based on
        /// which type of token is calling. PrioritySelectors, for example, may evaluate the current state of the
        /// world and choose the child with the highest priority. 
        /// </summary>
        /// <remarks>For now this just incrementally returns children. Later on it can be used to
        /// implement priority selectors</remarks>
        /// <returns>The integer index of the next child within the behavior tree. Returns -1 if 
        /// this node has no more children.</returns>
        public int GetNextChild()
        {
             
            int nextChildsIndexInList = m_PreviouslyRunChildren.Count; // Index of next child to run in Shape Token's ChildrenIndices.

            // All this node's children have been run, return -1 to indicate no more children.
            if (nextChildsIndexInList >= m_ReferenceToken.ChildrenIndices.Count)
            {
                return -1;
            }

            int nextChildToRun = m_ReferenceToken.ChildrenIndices[nextChildsIndexInList];

            m_PreviouslyRunChildren.Add(nextChildToRun);

            return nextChildToRun;
        }

        public static bool operator <(TraversalToken a, TraversalToken b)
        {
            return a.m_ReferenceToken.PositionInTree < b.m_ReferenceToken.PositionInTree;
        }
        public static bool operator <=(TraversalToken a, TraversalToken b)
        {
            return a.m_ReferenceToken.PositionInTree <= b.m_ReferenceToken.PositionInTree;
        }
        public static bool operator >(TraversalToken a, TraversalToken b)
        {
            //If a isn't less than or equal to b, it is greater.
            return !(a <= b);
        }
        public static bool operator >=(TraversalToken a, TraversalToken b)
        {
            return !(a < b);
        }
        public static bool operator ==(TraversalToken a, TraversalToken b)
        {
            return a.m_ReferenceToken.PositionInTree == b.m_ReferenceToken.PositionInTree;
        }
        public static bool operator !=(TraversalToken a, TraversalToken b)
        {   
            return !(a == b);
        }

        /// Equals(object)
        /// 
        /// <summary>
        /// Checks to see if two traversal tokens have the same reference token.
        /// </summary>
        /// <param name="token">The traversal token being checked for equality.</param>
        /// <returns>True if this token and the parameter token have the same reference token field. False if
        /// it does not, the parameter object is null, or the parameter object cannot be cast to a TraversalToken.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // If parameter obj cannot be cast to a TraversalToken, return false
            TraversalToken token = obj as TraversalToken;
            if ((System.Object)token == null)
            {
                return false;
            }

            return m_ReferenceToken == token.ReferenceToken;
        }

        /// Equals(TraversalToken)
        /// 
        /// <summary>
        /// Checks to see if two traversal tokens have the same reference token.
        /// </summary>
        /// <param name="token">The traversal token being checked for equality.</param>
        /// <returns>True if this token and the parameter token have the same reference token field, False if
        /// the parameter token is null or does not share the same reference token.</returns>
        public bool Equals(TraversalToken token)
        {
            // If the token passed is null, 
            if ((object)token == null)
            {
                return false;
            }

            return m_ReferenceToken == token.ReferenceToken;
        }

        /// Constructor
        /// 
        /// <summary>
        /// Instantiates a trivial instance of the TraversalToken. Not usable until it has been initialized.
        /// </summary>
        public TraversalToken()
        {
            m_ReferenceToken = new ShapeToken();
            m_CurrentState = TokenState.Uninitialized;
            m_CurrentRunningChild = -1;
            m_PreviouslyRunChildren = new List<int>();
        }

        public enum TokenState
        {
            Ready,
            Running,
            Success,
            CleanFail,
            UnexpectedFail,
            Uninitialized
        }

        public TokenState CurrentState
        { 
            get 
            { 
                return m_CurrentState; 
            }
            set 
            { 
                if (value != TokenState.Uninitialized)
                {
                    m_CurrentState = value;
                }
            }
        }

        public ShapeToken ReferenceToken
        {
            get
            {
                return m_ReferenceToken;
            }
        }

        public int CurrentRunningChild
        {
            get
            {
                return m_CurrentRunningChild;
            }
            set
            {
                //CurrentRunningChild must be one of this node's children. 
                if (m_ReferenceToken.ChildrenIndices.Contains(value))
                {
                    m_CurrentRunningChild = value;
                }
            }     
        }
        //-------------------------------------------------------------------------------------------------------------
        // Private
        //-------------------------------------------------------------------------------------------------------------

        ShapeToken     m_ReferenceToken;
        TokenState     m_CurrentState;
        int            m_CurrentRunningChild;
        List<int>      m_PreviouslyRunChildren;
    }
}
