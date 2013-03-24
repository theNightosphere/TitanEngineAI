//---------------------------------------------------------------------------------------------------------------------
//  Name:           Tree Interpreter [Titan.AI.TreeInterpreter]
//  Description:    The class responsible for traversal of the behavior tree. It takes as an input a list of AIActors,
//                  retrieves their associated behavior tree, and updates the actor data to reflect the current 
//                  traversal and prepare for the next traversal.               
//
//  Copyright (c) 2011-2013 Game Design & Development at UWM, All Rights Reserved
//---------------------------------------------------------------------------------------------------------------------
//  NOTES:
//
//  TODO: -Finish implementation of PerformLeafAction()
//        -Implement handling for UnexpectedFails via Decorators.
//        -Implement error handling for Uninitialized Tokens.
//        -During PerformLeafAction() check if node has deferred action and respond appropriately.
//     .
//---------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Titan.AI
{

    using DeferredAction = Titan.AI.ShapeToken.LeafActionDelegate;

    class TreeInterpreter
    {
        //-------------------------------------------------------------------------------------------------------------
        // Public
        //-------------------------------------------------------------------------------------------------------------

        /// Initialize
        /// 
        /// <summary>
        /// Initializes an instance of the TreeInterpreter. It also stores the BehaviorTree of the first 
        /// actor to be processed.
        /// </summary>
        /// <param name="actorsToProcess">A list of AI actors that this interpreter will process.</param>
        /// <returns>True if the interpreter is initialized properly, False if it is not.</returns>
        public bool Initialize(List<AIActor> actorsToProcess)
        {
            m_ActorsToUpdate = actorsToProcess;
            if (m_ActorsToUpdate.Count < 1) { return false; }

            string firstTree = m_ActorsToUpdate[0].TreeHandle;

            if (m_AIManager.IsActiveTree(firstTree))
            {
                m_CurrentTree = m_AIManager.GetTree(firstTree);
            }

            m_CurrentActor = m_ActorsToUpdate[0];

            return true;
        }

        /// UpdateAll
        /// 
        /// <summary>
        /// Iterates over the actors passed to the interpreter to update and calls update on each actor.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateAll(float deltaTime)
        {
            foreach (AIActor currentActor in m_ActorsToUpdate)
            {
                // Set the m_CurrentActor variable so that this object can be accessed by other functions without
                // needing to pass it as a parameter.
                m_CurrentActor = currentActor;
                // Initialize interpreter's m_PreviousTraversalStack with data from currentActor.
                LoadActorsPreviousTraversal(currentActor);
                
                string nextTree = currentActor.TreeHandle;
                // If we don't have a handle to the tree we will use in the coming traversal
                if (!m_CurrentTree.Handle.Equals(nextTree))
                {
                    // If the current tree is a tree we have loaded
                    if (m_AIManager.IsActiveTree(nextTree))
                    {
                        m_CurrentTree = m_AIManager.GetTree(nextTree);
                    }
                }
                // Have the current actor traverse their behavior tree. 
                Update(deltaTime);
                // Update actor's data from traversal and clean interpreter's data for next traversal.
                WriteCurrentTraversalAndClean(currentActor);
            }
        }

        /// Update
        /// 
        /// <summary>
        /// Traverses the behavior trees of an actor that has been passed to the interpreter.
        /// Update is called for each actor the TreeInterpreter is initialized with.
        /// </summary>
        /// <param name="deltaTime">The amount of time since the last call to update.</param>
        public void Update(float deltaTime)
        {
            int currentPosition = 0;

            while (currentPosition < m_CurrentTree.Nodes.Count)
            {
                TraversalToken currentToken = new TraversalToken();
                currentToken.Initialize(m_CurrentTree.Nodes[currentPosition]);

                // If the current token exists on the traversal stack, replace the current token with the 
                // one from the previous traversal. 
                if (m_PreviousTraversalStack.Peek() == currentToken)
                {
                    currentToken = m_PreviousTraversalStack.Dequeue();
                    m_CurrentTraversalStack.Push(currentToken);
                    currentPosition = currentToken.CurrentRunningChild;
                    continue;
                }

                switch (currentToken.CurrentState)
                {
                    case TraversalToken.TokenState.Ready:
                        // If token is leaf, run its action.
                        if (currentToken.ReferenceToken.CurrentTokenType == ShapeToken.TokenType.Leaf)
                        {
                            PerformLeafAction(currentToken, ref currentPosition);
                        }
                        else
                        {
                            HandleInnerToken(currentToken, ref currentPosition);
                        }
                        break;

                }
                // dive deeper into the tree.       
            }

        }

        public Queue<DeferredAction> DeferredActionQueue
        {
            get { return m_DeferredActionQueue; }
            private set { m_DeferredActionQueue = value; }
        }

        /// Constructor
        /// 
        /// <summary>
        /// Instantiates a trivial instance of the Tree Interpreter. Cannot be used until it is initialized.
        /// </summary>
        public TreeInterpreter()
        {
            m_PreviousTraversalStack = new Queue<TraversalToken>();
            m_CurrentTraversalStack = new Stack<TraversalToken>();
            m_ActorsToUpdate = new List<AIActor>();
            m_CurrentTree = new BehaviorTree();
            m_AIManager = new AIManager();
            m_NextTraversal = new List<TraversalToken>();
            m_CurrentActor = new AIActor();
        }

        //-------------------------------------------------------------------------------------------------------------
        // Private
        //-------------------------------------------------------------------------------------------------------------

        /// LoadActorsPreviousTraversal
        /// 
        /// <summary>
        /// Takes an actor and loads the TreeInterpreter with the previous traversal stack.
        /// The function compares the previous traversal stack to a list of deferred action results
        /// and replaces tokens with Running statuses with their return status if applicable.
        /// </summary>
        /// <param name="actorToLoadFrom">The AIActor used to load the TreeInterpreter.</param>
        private void LoadActorsPreviousTraversal(AIActor actorToLoadFrom)
        {
            Stack<TraversalToken> previousTraversalStack = actorToLoadFrom.PreviousTraversalStack;
            Stack<TraversalToken> temporaryStack = new Stack<TraversalToken>();
            List<TraversalToken> deferredActionResults = actorToLoadFrom.DeferredActionResults;

            while (previousTraversalStack.Count > 0)
            {
                TraversalToken currentToken = previousTraversalStack.Pop();
                // Checks if the deferred action results has a token with the same reference token as currentToken.
                // If such a token exists in results, then a Running token has a return value.
                if (deferredActionResults.Contains(currentToken))
                {
                    temporaryStack.Push(deferredActionResults.Find(x => x.Equals(currentToken)));
                }
                else // Otherwise push the current token.
                {
                    temporaryStack.Push(currentToken);
                }
            }

            while (temporaryStack.Count > 0)
            {
                m_PreviousTraversalStack.Enqueue(temporaryStack.Pop());
            }
        }

        /// PerformLeafAction
        /// 
        /// <summary>
        /// Performs the action of a leaf node and modifies the traversal stack and current traversal node accordingly.
        /// </summary>
        /// <param name="currentToken">A Traversal Token that has a CurrentType of TokenType.Leaf. If the token
        /// is not a leaf this function will attempt to call a delegate that does not exist.</param>
        private void PerformLeafAction(TraversalToken currentToken, ref int p_CurrentPosition)
        {
            // Here I can check if Leaf token has deferred action. If it does, I can emit deferred action request
            // and set current state to running. If it isn't, I call the delegate directly and let the return
            // value set the the CurrentState.
            if (currentToken.ReferenceToken.IsLeafActionImmediate)
            {
                currentToken.CurrentState = currentToken.ReferenceToken.LeafAction(m_CurrentActor.ActorsGameObject);
            }
            else
            {
                m_DeferredActionQueue.Enqueue(currentToken.ReferenceToken.LeafAction);
                currentToken.CurrentState = TraversalToken.TokenState.Running;
            }
            switch (currentToken.CurrentState)
            {

                case TraversalToken.TokenState.Success:
                    m_CurrentTraversalStack.Pop();
                    m_CurrentTraversalStack.Peek().CurrentState = TraversalToken.TokenState.Success;
                    break;

                case TraversalToken.TokenState.Running:
                    HaltTraversalWithRunning(currentToken, ref p_CurrentPosition);
                    break;

                case TraversalToken.TokenState.CleanFail:
                    m_CurrentTraversalStack.Pop();
                    m_CurrentTraversalStack.Peek().CurrentState = TraversalToken.TokenState.CleanFail;
                    break;

                case TraversalToken.TokenState.UnexpectedFail: // Should be handled by decorators. Not implemented yet.
                    goto default;
                case TraversalToken.TokenState.Uninitialized: // An erroneous state.
                    goto default;
                default:
                    break;

            }
        }

        /// HaltTraversalWithRunning
        /// 
        /// <summary>
        /// Called when a leaf node emits a deferred action request and traversal cannot progress without a
        /// return status. This function saves the current traversal state for use during the next traversal. 
        /// </summary>
        /// <entry>The currentToken has a Running TokenState.</entry>
        /// <exit>The current traversal stack is popped. The previous traversal stack has been cleared
        /// and filled with the traversal stack to be used in the next iteration. p_CurrentPosition
        /// has been increased to the length of the behavior tree in order to end traversal.</exit>
        /// <param name="currentToken">The TraversalToken that returned a Running token state.</param>
        /// <param name="p_CurrentPosition">A reference to the integer current position of tree traversal.
        /// This value is incremented to the end of the tree's range in order to end traversal.</param>
        private void HaltTraversalWithRunning(TraversalToken currentToken, ref int p_CurrentPosition)
        {
            m_NextTraversal.Clear();
            // Pop all tokens and set their current status to running. 
            while (m_CurrentTraversalStack.Count > 0)
            {
                TraversalToken token = m_CurrentTraversalStack.Pop();
                token.CurrentState = TraversalToken.TokenState.Running;
                m_NextTraversal.Add(m_CurrentTraversalStack.Pop());
            }
            // Sort the next traversal by token. 
            m_NextTraversal.Sort((x, y) => 
                x == null ? (y == null ? 0 : -1)
                : ( y == null ? 1 : x.ReferenceToken.PositionInTree.CompareTo(y.ReferenceToken.PositionInTree)));
            // Set the current position of traversal beyond the bounds of traversal in order to stop it.
            p_CurrentPosition = m_CurrentTree.Nodes.Count;
        }

        /// HandleInnerToken
        /// 
        /// <summary>
        /// Examines the current Token and determines whether to continue traversal with one of its children and
        /// set the Token's state to running, or whether the Token has no more children and has failed.
        /// </summary>
        /// <remarks>Passing a TraversalToken will not cause the program to crash but this represents
        /// an erroneous state. HandleInnerNode is not designed to properly handle Leaf nodes.</remarks>
        /// <param name="currentToken">The current TraversalToken. This must be a non-leaf node.</param>
        /// <param name="p_currentPosition">The integer index of the current position of the tree traversal.
        /// This value is passed by reference.</param>
        private void HandleInnerToken(TraversalToken currentToken, ref int p_currentPosition)
        {
            int nextChildIndex = currentToken.GetNextChild();
            // If no more children exist, this node fails.
            // Set CurrentState to CleanFail and let parent node deal with this on next iteration.
            if (nextChildIndex < 0)
            {
                currentToken.CurrentState = TraversalToken.TokenState.CleanFail;
            }
            else
            {
                currentToken.CurrentState = TraversalToken.TokenState.Running;
                p_currentPosition = nextChildIndex;
            }
        }

        /// WriteCurrentTraversalAndClean
        /// 
        /// <summary>
        /// At the end of traversing a behavior tree, the resulting TraversalStack is written to the Actor's previous
        /// traversal stack. It also ensures that m_PreviousTraversalStack, m_CurrentTraversalStack,
        /// and m_NextTraversal.
        /// </summary>
        /// <param name="actorToWriteTo">The AIActor whose data is being updated from the most recent tree
        /// traversal.</param>
        private void WriteCurrentTraversalAndClean(AIActor actorToWriteTo)
        {
            // Set actor's PreviousTraversalStack with the interpreter's next traversal stack.
            foreach (TraversalToken token in m_NextTraversal)
            {
                actorToWriteTo.PreviousTraversalStack.Push(token);
            }

            m_PreviousTraversalStack.Clear();
            m_CurrentTraversalStack.Clear();
            m_NextTraversal.Clear();
        }

        Queue<TraversalToken> m_PreviousTraversalStack;
        Queue<DeferredAction> m_DeferredActionQueue;
        Stack<TraversalToken> m_CurrentTraversalStack;
        List<AIActor>   m_ActorsToUpdate;
        List<TraversalToken>  m_NextTraversal;
        BehaviorTree          m_CurrentTree;
        AIManager             m_AIManager;
        AIActor               m_CurrentActor;
    }
}
