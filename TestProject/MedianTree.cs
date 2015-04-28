using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class MedianTree<T>
    {
        MedianTreeNode<T> root;
        IComparer<T> comparer;
        public MedianTree()
        {
            this.comparer = Comparer<T>.Default;
        }
        public MedianTree(IComparer<T> comparer)
        {
            this.comparer = comparer == null ? Comparer<T>.Default : comparer;
        }
        public void Add(T value, ulong count)
        {
            root = root.Add(value, count, comparer);
        }
        public ulong Count
        { get { return root.Count(); } }
    }
    public class MedianTreeNode<T>
    {
        //binary tree fields
        public T Key;
        public MedianTreeNode<T> Left;
        public MedianTreeNode<T> Right;
        //median enhancement
        public ulong CountLeft;
        public ulong CountThis;
        public ulong CountRight;
        //double link list enhancement
        public bool LeftIsChild;
        public bool RightIsChild;
    }
    public static class MedianTreeExt
    {
        public static ulong Count<T>(this MedianTreeNode<T> tree)
        {
            if(tree == null)
                return 0;
            checked
            {
                return tree.CountLeft + tree.CountThis + tree.CountRight;
            }
        }
        public static ulong CountLeft<T>(this MedianTreeNode<T> tree)
        {
            if(tree == null)
                return 0;
            return tree.CountLeft;
        }
        public static ulong CountThis<T>(this MedianTreeNode<T> tree)
        {
            if(tree == null)
                return 0;
            return tree.CountThis;
        }
        public static ulong CountRight<T>(this MedianTreeNode<T> tree)
        {
            if(tree == null)
                return 0;
            return tree.CountRight;
        }
        public static ulong CountLeftLeft<T>(this MedianTreeNode<T> tree)
        {
            if(tree == null)
                return 0;
            return tree.Left.CountLeft();
        }
        public static ulong CountLeftThis<T>(this MedianTreeNode<T> tree)
        {
            if(tree == null)
                return 0;
            return tree.Left.CountThis();
        }
        public static ulong CountLeftRight<T>(this MedianTreeNode<T> tree)
        {
            if(tree == null)
                return 0;
            return tree.Left.CountRight();
        }
        public static ulong CountRightLeft<T>(this MedianTreeNode<T> tree)
        {
            if(tree == null)
                return 0;
            return tree.Right.CountLeft();
        }
        public static ulong CountRightThis<T>(this MedianTreeNode<T> tree)
        {
            if(tree == null)
                return 0;
            return tree.Right.CountThis();
        }
        public static ulong CountRighttRight<T>(this MedianTreeNode<T> tree)
        {
            if(tree == null)
                return 0;
            return tree.Right.CountRight();
        }
        public static bool SumGreaterThanZero(ulong[] positives, ulong[] negatives)
        {
            bool plusSign = true;
            ulong sum = 0;
            int i = 0;
            int j = 0;
            int cp = positives.Length;
            int cn = negatives.Length;
            while(i < cp && j < cn)
            {
                if(sum > 0)
                {
                    if(plusSign)
                    {
                        if(j < cn)
                        {
                            ulong t = negatives[j];
                            if(sum >= t)
                            {
                                sum -= t;
                                j++;
                                continue;
                            }
                            else
                            {
                                sum = t - sum;
                                plusSign = false;
                                j++;
                                continue;
                            }
                        }
                        break;
                    }
                    else//!plusSign
                    {
                        if(i < cp)
                        {
                            ulong t = positives[i];
                            if(sum > t)
                            {
                                sum -= t;
                                j++;
                                continue;
                            }
                            else
                            {
                                sum = t - sum;
                                plusSign = true;
                                j++;
                                continue;
                            }
                        }
                        break;
                    }
                }
                else//sum==0
                {
                    if(i < cp)
                    {
                        sum = positives[i];
                        plusSign = true;
                        i++;
                        continue;
                    }
                    if(j < cn)
                    {
                        sum = negatives[j];
                        plusSign = false;
                        j++;
                        continue;
                    }
                }
            }
            return plusSign && sum > 0;
        }
        public static MedianTreeNode<T> Add<T>(this MedianTreeNode<T> tree, T value, ulong count, IComparer<T> comparer)
        {
            if(tree == null)
                return new MedianTreeNode<T>() { Key = value, CountThis = count };
            MedianTreeNode<T> current = tree;
            Stack<MedianTreeNode<T>> parents = new Stack<MedianTreeNode<T>>();
            bool flag = true;
            do
            {
                int cmp = comparer.Compare(value, current.Key);
                if(cmp < 0)
                {
                    checked
                    {
                        current.CountLeft += count;
                    }
                    parents.Push(current);
                    if(current.Left == null)
                    {
                        current.Left = new MedianTreeNode<T>() { Key = value, CountThis = count };
                        flag = false;
                    }
                    current = current.Left;
                }
                else if(cmp > 0)
                {
                    checked
                    {
                        current.CountRight += count;
                    }
                    parents.Push(current);
                    if(current.Right == null)
                    {
                        current.Right = new MedianTreeNode<T>() { Key = value, CountThis = count };
                        flag = false;
                    }
                    current = current.Right;
                }
                else
                {
                    checked
                    {
                        current.CountThis += count;
                    }
                    flag = false;
                }
            } while(flag);
            MedianTreeNode<T> result = current;
            //now balance the tree
            while(parents.Count > 0)
            {
                MedianTreeNode<T> parent = parents.Pop();
                if(current == parent.Left)
                {
                    if(SumGreaterThanZero(
                        new ulong[] { current.CountThis, current.CountLeft, current.CountLeft },
                        new ulong[] { parent.CountThis, parent.CountRight, parent.CountRight }
                        ))//rotate right
                    {

                    }
                }
                else
                {
                    if(SumGreaterThanZero(
                        new ulong[] { current.CountThis, current.CountRight, current.CountRight },
                        new ulong[] { parent.CountThis, parent.CountLeft, parent.CountLeft }
                        ))//rotate left
                    {

                    }
                }
            }
            return result;
        }
    }
}
