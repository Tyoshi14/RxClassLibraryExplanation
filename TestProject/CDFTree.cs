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

        #region + Test red-black tree +
        //为了检验红黑树结构正确性编写了此测试伪代码

        //首先约定树的简易表示方法（因为画树结构比较困难）
        //用单个字母表示树的节点
        //为了方便讨论，约定树节点的表示字母的顺序和树节点键值顺序相同
        //用括号表示树的层次
        //下面用例子解释上面的概念
        //A，可以表示一个节点，也可以表示根节点为A的树，且A的左节点和右节点未作限制，可以为空，也可以很复杂而省略未写
        //(A)B，可以表示根节点为B的树，且B的左节点为A
        //A(B)，可以表示根节点为A的树，且A的右节点为B
        //ABC，可以表示根节点为B的树，且B的左节点为A，B的右节点为C
        //(ABC)DE的根是D，D的左节点是树ABC，D的右节点是E
        //AB(CDE)的根是B，B的左节点是A，B的右节点是树CDE
        //值得注意的是，这种表示方法删除括号后获得的节点序列和中序遍历结果相同
        //另外需要注意的是如果去掉括号后的序列未按字母顺序则原树不是合法的树

        //新节点插入
        //我们这里的插入为简单搜索插入，不做平衡处理
        //这样，给定插入顺序就应该得到确定的树结构，我们用+表示插入，用N表示空树
        //则应当有下面的测试可以编写
        //N+A=>A
        //A+B=>A(B)
        //B+A=>(A)B
        //当连续插入且不影响输出树结构时，将连续插入且顺序可调的组用方括号括起并省略+
        //B[AC]=>ABC
        //B[CA]=>ABC
        //B[AF][DG][CE]=>AB((CDE)FG)
        //D[BF][ACEG]=>(ABC)D(EFG)
        //不难发现，这种插入顺序的表示方法，如果每个顺序可调组我们按字母顺序列出，则整个表示和从根开始广度优先遍历的顺序相同



        //左旋和右旋
        //L()为左旋函数，输入为一个树旋转前的根节点，输出为该树左旋后的新根节点
        //因此L(AB(CDE))=>(ABC)DE
        //与左旋函数类似，R()为右旋函数
        //R((ABC)DE)=>AB(CDE)

        //平衡算法
        //平衡算法的核心是要获得(ABC)D(EFG)的平衡形式
        //L(AB(CD(EFG)))=>(ABC)D(EFG)
        //R(((ABC)DE)FG))=>(ABC)D(EFG)
        //L(R(AB((CDE)FG)))=>(ABC)D(EFG)
        //R(L((AB(CDE))FG))=>(ABC)D(EFG)
        //前两个是所谓zig-zig和zag-zag树，只需旋转一次即可平衡
        //后两个是所谓zig-zag和zag-zig树，需要先旋转成zig-zig和zag-zag再旋转平衡

        //按上面的方法表示树，节点字母用toString函数获得
        //测试时T可以直接为string，即用字母作为键值
        //这样通过比较字符串就可以直接比较树结构，非常方面，输出也比较直观
        public static string Serielize(CDFTree<T> tree, Func<T, string> toString)
        {
            return serielizeHelper(tree.root, toString).Item1;
        }
        private static Tuple<string, bool> serielizeHelper(Node node, Func<T, string> toString)
        {
            if(node == null)
                return Tuple.Create("", true);
            string key = toString(node.Key);
            if(node.IsRed)
                key = key.ToLower();
            if(node.Left == null && node.Right == null)
                return Tuple.Create(key, true);
            if(node.Left == null && node.Right != null)
                return Tuple.Create(key + "(" + serielizeHelper(node.Right, toString).Item1 + ")", false);
            if(node.Left != null && node.Right == null)
                return Tuple.Create("(" + serielizeHelper(node.Left, toString).Item1 + ")" + key, false);
            Tuple<string, bool> left = serielizeHelper(node.Left, toString);
            Tuple<string, bool> right = serielizeHelper(node.Right, toString);
            string result = "";
            if(left.Item2)
                result += left.Item1;
            else
                result += "(" + left.Item1 + ")";
            result += key;
            if(right.Item2)
                result += right.Item1;
            else
                result += "(" + right.Item1 + ")";
            return Tuple.Create(result, false);
        }

        //利用插入顺序表示构造树
        public static CDFTree<char> Create(string input)
        {
            var result = new CDFTree<char>();
            foreach(var k in input)
                if(k >= 'A' && k <= 'Z')
                    result.Add(k);
            return result;
        }

        public List<T> getTreeInOrderWalk()
        {
            List<T> list = new List<T>();
            iterateElement(root, ref list);
            return list;
        }

        public class returnNode
        {
            public T key;
            public bool isRed;
            public returnNode(T _key, bool _isRed)
            {
                key = _key;
                isRed = _isRed;
            }
        }
        public List<returnNode> getTreeInLayer()
        {
            Node signal = new Node(default(T), 10, true);
            returnNode returnNodeSign = new returnNode(default(T), true);
            List<returnNode> list = new List<returnNode>();
            Queue<Node> quene = new Queue<Node>();

            quene.Enqueue(root);
            quene.Enqueue(signal);

            while(quene.Count > 0)
            {
                var node = quene.Dequeue();

                if(node == signal && quene.Count > 0)
                {
                    list.Add(returnNodeSign);
                    quene.Enqueue(signal);
                    continue;
                }
                list.Add(new returnNode(node.Key, node.IsRed));
                if(node.Left != null)
                {
                    quene.Enqueue(node.Left);
                }
                if(node.Right != null)
                {
                    quene.Enqueue(node.Right);
                }
            }

            return list;
        }

        // In-order walk to get the tree.
        private void iterateElement(Node node, ref List<T> list)
        {
            if(node == null)
                return;
            if(node.Left != null)
            {
                iterateElement(node.Left, ref list);
            }
            list.Add(node.Key);
            if(node.Right != null)
            {
                iterateElement(node.Right, ref list);
            }

        }


        #endregion


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
            if(node.CountThis > 1)
                return;
            while(node.Parent != null && node.Parent != root && node.Parent.IsRed)
            {
                Node parentNode = node.Parent;

                if(parentNode.Parent == parentNode.Parent.Left)
                {
                    Node uncleRight = parentNode.Parent.Right;
                    if(uncleRight != null && uncleRight.IsRed)
                    {
                        parentNode.IsRed = false;
                        uncleRight.IsRed = false;
                        parentNode.Parent.IsRed = true;
                        node = parentNode.Parent;
                    }
                    else if(node == parentNode.Right)
                    {
                        node = parentNode;
                        Left_rotation(node);
                    }
                    else
                    {
                        parentNode.IsRed = false;
                        parentNode.Parent.IsRed = true;
                        Right_rotation(parentNode.Parent);
                    }
                }
                else
                {
                    Node uncleLeft = parentNode.Parent.Left;
                    if(uncleLeft != null && uncleLeft.IsRed)
                    {
                        parentNode.IsRed = false;
                        uncleLeft.IsRed = false;
                        parentNode.Parent.IsRed = true;
                        node = parentNode.Parent;
                    }
                    else if(node == parentNode.Left)
                    {
                        node = parentNode;
                        Right_rotation(node);
                    }
                    else
                    {
                        parentNode.IsRed = false;
                        parentNode.Parent.IsRed = true;
                        Left_rotation(parentNode.Parent);
                    }
                }
            }

            root.IsRed = false;
        }


        private void Right_rotation(Node node)
        {
            Node leftChildren = node.Left;
            node.Left = leftChildren.Right;
            if(leftChildren.Right != null)
            {
                leftChildren.Right.Parent = node;
            }

            leftChildren.Parent = node.Parent;
            if(node.Parent == null)
            {
                root = leftChildren;
            }
            else if(node.Parent.Left == node)
            {
                node.Parent.Left = leftChildren;
            }
            else
            {
                node.Parent.Right = leftChildren;
            }

            leftChildren.Right = node;
            node.Parent = leftChildren;
        }


        private void Left_rotation(Node node)
        {
            Node rightChild = node.Right;
            node.Right = rightChild.Left;
            if(rightChild.Left != null)
            {
                rightChild.Left.Parent = node;
            }
            rightChild.Parent = node.Parent;

            if(node.Parent == null)
            {
                root = rightChild;
            }
            else if(node == node.Parent.Left)
            {
                node.Parent.Left = rightChild;
            }
            else
            {
                node.Parent.Right = rightChild;
            }
            rightChild.Left = node;
            node.Parent = rightChild;
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
            double k = (double)node.CountLessAndEqual / SampleSize;
            if(p < k && node.Left != null)
                return ICDFHelper(node.Left, p);
            if(p > k)
            {
                if(node.Right != null)
                    return ICDFHelper(node.Right, p);
                if(node.Parent != null && node.Parent.Left == node)
                    return node.Parent.Key;
            }
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
