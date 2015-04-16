using System;
using System.Collections.Generic;

namespace TestProject
{
    // 学习北大张铭数据结构
    // http://v.youku.com/v_show/id_XMTcwMzI3MDQ0.html?f=4432970
    // 学习MIT算法导论
    // http://open.163.com/special/opencourse/algorithms.html
    // 特别是[第10集] 平衡搜索树
    // http://open.163.com/movie/2010/12/9/J/M6UTT5U0I_M6V2TJ49J.html
    // 以及[第11集] 扩充的数据结构、动态有序统计和区间树
    // http://open.163.com/movie/2010/12/G/0/M6UTT5U0I_M6V2TSIG0.html
    // 学习.NET Framework SortedSet类
    // http://referencesource.microsoft.com/System/compmod/system/collections/generic/sortedset.cs.html
    // 以红黑树为基础利用扩充的数据结构的思路完成累计分布函数树（CDFTree）类
    // 我给你写了个半成品，你补充完整并做测试。
    /// <summary>
    /// 以红黑树为基础的累计分布函数专用动态数据结构
    /// </summary>
    public class CDFTree<T>
    {
        Node root;
        IComparer<T> comparer;
        public CDFTree()
        {
            this.comparer = Comparer<T>.Default;
        }
        public CDFTree(IComparer<T> comparer)
        {
            if(comparer == null)
                this.comparer = Comparer<T>.Default;
            else
                this.comparer = comparer;
        }
        /// <summary>
        /// 已储存的样本总数
        /// </summary>
        public ulong SampleSize
        {
            get
            {
                if(root == null)
                    return 0;
                return root.Count;
            }
        }
        public void Add(T sample, ulong number = 1)
        {
            //复杂性应当是以log(n)找到叶节点并完成样本新增后再原路返回至根节点
            Balance(AddHelper(ref root, sample, number, null));
            //应为CDFTree的特殊用途，没有必要实现删除功能。
        }
        public double CDF(T value)
        {
            return CDFHelper(root, value);
        }
        public T ICDF(double p)
        {
            return ICDFHelper(root, p);
        }
        public ulong Frequency(T value)
        {
            return FrequencyHelper(root, value);
        }
        /// <summary>
        /// 自叶向根完成树的平衡
        /// 目前不用考虑计数字段CountThis和count属性们，只需完成平衡算法
        /// 测试通过后在另起一个类完成count属性们去递归的事情
        /// 红黑树平衡相对比较简单，看MIT教程即可
        /// .NET的代码可以作为参考，不必模仿
        /// </summary>
        /// <param name="node"></param>
        internal void Balance(Node node)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 储存新样本的方法实现示意
        /// </summary>
        /// <param name="node">初始插入节点</param>
        /// <param name="sample">样本的值</param>
        /// <param name="number">样本频次（便于高效处理多次出现的相同样本值，因此默认为1）</param>
        /// <returns>最终插入节点</returns>
        internal Node AddHelper(ref Node node, T sample, ulong number, Node parent)
        {
            if(node == null)
            {
                node = new Node(sample, number);//note that node is by ref
                node.Parent = parent;
                return node;
            }
            int cmp = comparer.Compare(sample, node.Key);
            if(cmp < 0)
                return AddHelper(ref node.Left, sample, number, node);
            if(cmp > 0)
                return AddHelper(ref node.Right, sample, number, node);
            //cmp==0
            checked
            {
                node.CountThis += number;
            }
            return node;
        }
        /// <summary>
        /// 查询累计概率的方法示意，参考
        /// http://en.wikipedia.org/wiki/Cumulative_distribution_function
        /// </summary>
        /// <param name="node">初始查询节点</param>
        /// <param name="value">随机变量取值</param>
        /// <returns>累计概率</returns>
        internal double CDFHelper(Node node, T value)
        {
            if(node == null)
                return double.NaN;
            int cmp = comparer.Compare(value, node.Key);
            if(cmp < 0)
            {
                if(node.Left == null)
                    return (double)node.CountLess / SampleSize;
                return CDFHelper(node.Left, value);
            }
            if(cmp > 0)
            {
                if(node.Right == null)
                    return (double)node.CountLessAndEqual / SampleSize;
                return CDFHelper(node.Right, value);
            }
            return (double)node.CountLessAndEqual / SampleSize;
        }
        /// <summary>
        /// 查询累计概率反函数的方法示意，参考
        /// http://en.wikipedia.org/wiki/Quantile_function
        /// </summary>
        /// <param name="node">初始查询节点</param>
        /// <param name="p">累计概率</param>
        /// <returns>随机变量取值</returns>
        internal T ICDFHelper(Node node, double p)
        {
            if(root == null)
                throw new NotSupportedException();
            double k = ((double)node.CountLeft + node.CountThis) / node.Count;
            if(p < k && node.Left != null)
                return ICDFHelper(node.Left, p);
            if(p > k && node.Right != null)
                return ICDFHelper(node.Right, p);
            return node.Key;
        }
        internal ulong FrequencyHelper(Node node, T value)
        {
            if(node == null)
                return 0;
            int cmp = comparer.Compare(value, node.Key);
            if(cmp < 0)
            {
                if(node.Left == null)
                    return 0;
                return FrequencyHelper(node.Left, value);
            }
            if(cmp > 0)
            {
                if(node.Right == null)
                    return 0;
                return FrequencyHelper(node.Right, value);
            }
            return node.CountThis;
        }
        /// <summary>
        /// 为累计分布函数扩充的红黑树
        /// </summary>
        internal class Node
        {
            public T Key;//key value
            public ulong CountThis;//number of samples that Compare(smaple,Key)==0
            public Node Left;//left subtree to store Compare(smaple,Key)<0
            public Node Right;//right subtree to store Compare(sample,Key)>0
            public Node Parent;//for easier tree traversal 这样就可以写递归的算法 如果能够写出不使用的更好
            public bool IsRed;//flag for black or red
            public Node(T sample, ulong number = 1, bool isRed = true)
            {
                this.Key = sample;
                this.CountThis = number;
                this.IsRed = isRed;// The default color will be red, we never need to create a black node directly.
            }
            #region properties
            //现在这种实现递归起来很要命，但实现起来最接近标准红黑树，是个不错的起点。
            //另外一种办法就是缓存计算结果并在样本发生变化时调整，这样整个CDFTree各部分都需要有较大改动。
            public ulong CountLeft//total number of samples stored in the left subtree
            {
                get
                {
                    if(Left != null)
                        return Left.Count;
                    return 0;
                }
            }
            public ulong Count//total number of samples in this tree
            {
                get
                {
                    checked
                    {
                        return CountLeft + CountThis + CountRight;
                    }
                }
            }
            public ulong CountRight//total number of samples stored in the right subtree
            {
                get
                {
                    if(Right != null)
                        return Right.Count;
                    return 0;
                }
            }
            public ulong CountLess
            {
                get
                {
                    if(Parent != null && Parent.Right == this)
                        checked
                        {
                            return Parent.CountLessAndEqual + CountLeft;
                        }
                    return CountLeft;
                }
            }
            public ulong CountLessAndEqual
            {
                get
                {
                    checked
                    {
                        return CountLess + CountThis;
                    }
                }
            }
            #endregion
        }
    }
}
