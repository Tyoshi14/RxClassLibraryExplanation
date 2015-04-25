using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestProject
{
    //
    // A binary search tree is a red-black tree if it satisfies the following red-black properties:
    // 1. Every node is either red or black
    // 2. Every leaf (nil node) is black
    // 3. If a node is red, then both its children are black
    // 4. Every simple path from a node to a descendant leaf contains the same number of black nodes
    // 
    // The basic idea of red-black tree is to represent 2-3-4 tree as standard BST  
    // 2-node will be represented as:          1B2   
    // 3-node will be represented as:          (1R2)B3  1B(2R3)    
    // 4-node will be represented as:          (1R2)B(3R4)
    // For a detailed description of the algorithm, take a look at "Algorithms" by Robert Sedgewick.
    //
    internal enum TreeRotation
    {
        LeftRotation = 1,
        RightRotation = 2,
        RightLeftRotation = 3,
        LeftRightRotation = 4,
    }
    public class SortedSet<T>
    {
        #region local variables/constants
        Node root;
        IComparer<T> comparer;
        #endregion

        #region Constructors
        public SortedSet(IComparer<T> comparer = null)
        {
            if(comparer == null)
                this.comparer = Comparer<T>.Default;
            else
                this.comparer = comparer;
        }
        #endregion

        #region Tree Walk Helpers
        internal bool InOrderTreeWalk(Func<Node, bool> action)
        {
            return InOrderTreeWalk(action, false);
        }
        internal virtual bool InOrderTreeWalk(Func<Node, bool> action, bool reverse)
        {
            if(root == null)
                return true;
            Stack<Node> stack = new Stack<Node>();
            Node current = root;
            while(current != null)
            {
                stack.Push(current);
                current = (reverse ? current.Right : current.Left);
            }
            while(stack.Count != 0)
            {
                current = stack.Pop();
                if(!action(current))
                    return false;
                current = (reverse ? current.Left : current.Right);
                while(current != null)
                {
                    stack.Push(current);
                    current = (reverse ? current.Right : current.Left);
                }
            }
            return true;
        }
        internal virtual bool BreadthFirstTreeWalk(Func<Node, bool> action)
        {
            if(root == null)
                return true;
            Queue<Node> processQueue = new Queue<Node>();
            processQueue.Enqueue(root);
            Node current;
            while(processQueue.Count != 0)
            {
                current = processQueue.Dequeue();
                if(!action(current))
                    return false;
                if(current.Left != null)
                    processQueue.Enqueue(current.Left);
                if(current.Right != null)
                    processQueue.Enqueue(current.Right);
            }
            return true;
        }
        #endregion

        #region ICollection<T> Members
        public bool Add(T item)
        {
            return AddIfNotPresent(item);
        }
        internal virtual bool AddIfNotPresent(T item)
        {
            if(root == null)// empty tree
            {
                root = new Node(item, false);
                return true;//inserted
            }

            Node current = root;
            Node parent = null;
            Node grandParent = null;
            Node greatGrandParent = null;

            //even if we don't actually add to the set, we may be altering its structure (by doing rotations
            //and such). so update version to disable any enumerators/subsets working on it

            int cmp = 0;
            while(current != null)
            {
                cmp = comparer.Compare(item, current.Item);
                if(cmp == 0)
                {
                    // We could have changed root node to red during the search process.
                    // We need to set it to black before we return.
                    root.IsRed = false;
                    return false;//duplicated
                }

                // Search for a node at bottom to insert the new node. 
                // If we can guanratee the node we found is not a 4-node, it would be easy to do insertion.
                // We split 4-nodes into two 2-nodes along the search path.
                if(Is4Node(current))
                {
                    Split4Node(current);
                    // We could have introduced two consecutive red nodes after split. Fix that by rotation.
                    if(IsRed(parent))
                        InsertionBalance(current, ref parent, grandParent, greatGrandParent);
                }
                //go deeper
                greatGrandParent = grandParent;
                grandParent = parent;
                parent = current;
                current = (cmp < 0) ? current.Left : current.Right;
            }
            //now current==null
            Debug.Assert(parent != null, "Parent node cannot be null here!");
            // ready to insert the new node
            // The default color will be red, we never need to create a black node directly.
            current = new Node(item, true);
            if(cmp < 0)
                parent.Left = current;
            else
                parent.Right = current;

            // the new node will be red, so we will need to adjust the colors if parent node is also red
            if(parent.IsRed)
                InsertionBalance(current, ref parent, grandParent, greatGrandParent);

            // We could have changed root node to red during the search process.
            // We need to set it to black before we return.
            root.IsRed = false;
            return true;
        }
        public virtual void Clear()
        {
            root = null;
        }
        #endregion

        #region Tree Specific Operations

        private void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent)
        {
            Debug.Assert(grandParent != null, "Grand parent cannot be null here!");
            bool parentIsOnRight = (grandParent.Right == parent);
            bool currentIsOnRight = (parent.Right == current);

            Node newChildOfGreatGrandParent;
            if(parentIsOnRight == currentIsOnRight)
            { // same orientation, single rotation
                newChildOfGreatGrandParent = currentIsOnRight ? RotateLeft(grandParent) : RotateRight(grandParent);
            }
            else
            {  // different orientaton, double rotation
                newChildOfGreatGrandParent = currentIsOnRight ? RotateLeftRight(grandParent) : RotateRightLeft(grandParent);
                // current node now becomes the child of greatgrandparent 
                parent = greatGrandParent;
            }
            // grand parent will become a child of either parent of current.
            grandParent.IsRed = true;
            newChildOfGreatGrandParent.IsRed = false;

            ReplaceChildOfNodeOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
        }
        // Replace the child of a parent node. 
        // If the parent node is null, replace the root.        
        private void ReplaceChildOfNodeOrRoot(Node parent, Node child, Node newChild)
        {
            if(parent != null)
            {
                if(parent.Left == child)
                    parent.Left = newChild;
                else
                    parent.Right = newChild;
            }
            else
                root = newChild;
        }

        private static bool Is2Node(Node node)
        {
            Debug.Assert(node != null, "node cannot be null!");
            return IsBlack(node) && IsNullOrBlack(node.Left) && IsNullOrBlack(node.Right);
        }
        private static bool Is4Node(Node node)
        {
            return IsRed(node.Left) && IsRed(node.Right);
        }
        private static bool IsRed(Node node)
        {
            return (node != null && node.IsRed);//leaf is black
        }
        private static bool IsBlack(Node node)
        {
            return (node != null && !node.IsRed);
        }
        private static bool IsNullOrBlack(Node node)
        {
            return (node == null || !node.IsRed);
        }



        private static Node RotateLeft(Node node)
        {
            Node x = node.Right;
            node.Right = x.Left;
            x.Left = node;
            return x;
        }
        private static Node RotateLeftRight(Node node)
        {
            Node child = node.Left;
            Node grandChild = child.Right;

            node.Left = grandChild.Right;
            grandChild.Right = node;
            child.Right = grandChild.Left;
            grandChild.Left = child;
            return grandChild;
        }
        private static Node RotateRight(Node node)
        {
            Node x = node.Left;
            node.Left = x.Right;
            x.Right = node;
            return x;
        }
        private static Node RotateRightLeft(Node node)
        {
            Node child = node.Right;
            Node grandChild = child.Left;

            node.Right = grandChild.Left;
            grandChild.Left = node;
            child.Left = grandChild.Right;
            grandChild.Right = child;
            return grandChild;
        }

        private static void Split4Node(Node node)
        {
            node.IsRed = true;
            node.Left.IsRed = false;
            node.Right.IsRed = false;
        }

        #endregion

        #region Helper Classes
        internal class Node
        {
            public bool IsRed;
            public T Item;
            public Node Left;
            public Node Right;
            public Node(T item, bool isRed)
            {
                Item = item;
                IsRed = isRed;
            }
        }
        #endregion
    }
}
