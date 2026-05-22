using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wcs.Framework
{
    /// <summary>
    /// 邻接列表.用于创建完整的连通网络
    /// </summary>
    /// <typeparam name="T">泛型参数</typeparam>
    public class AdjacencyList<T>
    {
        /// <summary>
        /// 图的顶点集合
        /// </summary>
        List<Vertex<T>> items;
        public AdjacencyList() : this(10) { } 
        public AdjacencyList(int capacity)
        {
            items = new List<Vertex<T>>(capacity);
        }
        /// <summary>
        /// 添加一个顶点(不允许插入重复值)
        /// </summary>
        /// <param name="item"></param>
        public void AddVertex(T item)
        {   //
            if (Contains(item))
            {
                throw new ArgumentException("插入了重复顶点！");
            }
            items.Add(new Vertex<T>(item));
        }
        /// <summary>
        /// 添加无向边
        /// </summary>
        public void AddEdge(T from, T to)
        {
            Vertex<T> fromVer = Find(from); //找到起始顶点
            if (fromVer == null)
            {
                throw new ArgumentException("头顶点并不存在！");
            }
            Vertex<T> toVer = Find(to); //找到结束顶点
            if (toVer == null)
            {
                throw new ArgumentException("尾顶点并不存在！");
            }
            //无向边的两个顶点都需记录边信息
            AddDirectedEdge(fromVer, toVer);
            AddDirectedEdge(toVer, fromVer);
        }
        /// <summary>
        /// 添加有向边
        /// </summary>
        public void AddDirectedEdge(T from, T to)
        {
            Vertex<T> fromVer = Find(from); //找到起始顶点
            if (fromVer == null)
            {
                throw new ArgumentException("头顶点并不存在！");
            }
            Vertex<T> toVer = Find(to); //找到结束顶点
            if (toVer == null)
            {
                throw new ArgumentException("尾顶点并不存在！");
            }
            //无向边的两个顶点都需记录边信息
            AddDirectedEdge(fromVer, toVer);
        }
        /// <summary>
        /// 查找图中是否包含某项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            foreach (Vertex<T> v in items)
            {
                if (v.data.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 查找指定项并返回
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private Vertex<T> Find(T item)
        {
            foreach (Vertex<T> v in items)
            {
                if (v.data.Equals(item))
                {
                    return v;
                }
            }
            return null;
        }
        /// <summary>
        /// 添加有向边
        /// </summary>
        /// <param name="fromVer"></param>
        /// <param name="toVer"></param>
        private void AddDirectedEdge(Vertex<T> fromVer, Vertex<T> toVer)
        {
            if (fromVer.firstEdge == null) //无邻接点时
            {
                fromVer.firstEdge = new Node(toVer);
            }
            else
            {
                Node tmp, node = fromVer.firstEdge;
                do
                {   //检查是否添加了重复边
                    if (node.adjvex.data.Equals(toVer.data))
                    {
                        throw new ArgumentException("添加了重复的边！");
                    }
                    tmp = node;
                    node = node.next;
                } while (node != null);
                tmp.next = new Node(toVer); //添加到链表未尾
            }
        }
        /// <summary>
        /// 指示一条有向边是否已存在
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool DirectedEdgeIsContains(T from, T to)
        {
            Vertex<T> fromVer = Find(from); //找到起始顶点
            if (fromVer == null)
            {
                throw new ArgumentException("头顶点并不存在！");
            }
            Vertex<T> toVer = Find(to); //找到结束顶点
            if (toVer == null)
            {
                throw new ArgumentException("尾顶点并不存在！");
            }


            if (fromVer.firstEdge == null) //无邻接点时
            {
                return false;
            }
            else
            {
                Node tmp, node = fromVer.firstEdge;
                do
                {   //检查是否添加了重复边
                    if (node.adjvex.data.Equals(toVer.data))
                    {
                        return true;
                    }
                    tmp = node;
                    node = node.next;
                } while (node != null);
            }

            return false;
        }
        /// <summary>
        /// 仅用于测试
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {   //打印每个节点和它的邻接点
            string s = string.Empty;
            foreach (Vertex<T> v in items)
            {
                s += v.data.ToString() + ":";
                if (v.firstEdge != null)
                {
                    Node tmp = v.firstEdge;
                    while (tmp != null)
                    {
                        s += tmp.adjvex.data.ToString();
                        tmp = tmp.next;
                    }
                }
                s += "\r\n";
            }
            return s;
        }

        List<List<T>> m_Nets;
        ReaderWriterLockSlim net_lock = new ReaderWriterLockSlim();
        /// <summary>
        /// 获取图内所有顶点最深可达的路径集合
        /// </summary>
        public List<List<T>> Nets
        {
            get
            {
                if (m_Nets == null)
                {
                    net_lock.EnterWriteLock();
                    try
                    {
                        //得到锁后如果已经被更新过了，则直接返回
                        if (m_Nets != null) return m_Nets;

                        m_Nets = new List<List<T>>();
                        foreach (var item in items)
                        {
                            InitVisited();              //将visited标志全部置为false
                            List<T> net = new List<T>();
                            DFSFindNets(item, ref net); //从第一个顶点开始遍历
                        }
                    }
                    finally
                    {
                        net_lock.ExitWriteLock();
                    }
                }


                return m_Nets;
            }
        }
        private void DFSFindNets(Vertex<T> v, ref List<T> net) //使用递归进行深度优先遍历
        {
            #warning 注释这行，改用在内部还原访问点后，找到的路径有可能陷入无限回路
            //v.visited = true; //将访问标志设为true

            net.Add(v.data);
            T[] org = (T[])net.ToArray().Clone();

            Node node = v.firstEdge;

            while (node != null) //访问此顶点的所有邻接点
            {
                v.visited = true; //将访问标志设为true
                //如果邻接点未被访问，则递归访问它的边
                if (!node.adjvex.visited)
                {
                    DFSFindNets(node.adjvex, ref net); //递归
                }
                node = node.next; //访问下一个邻接点
                net = ((T[])org.Clone()).ToList();

                v.visited = false; //将访问标志设为true
            }

            if (v.firstEdge == null)
            {
                Nets.Add(net);
            }
            //if (v.firstEdge == null && net.Count > 1)
            //{
            //    Nets.Add(net);
            //}
        }

        public void DFSTraverse() //深度优先遍历
        {

            List<string> list = new List<string>();

            //foreach (var item in items)
            //{
            //    InitVisited(); //将visited标志全部置为false
            //    list = new List<string>();
            //    DFS(item, ref list); //从第一个顶点开始遍历
            //}
            found = new Dictionary<string, bool>();
            DFS(items[1], ref list); //从第一个顶点开始遍历
        }

        Dictionary<string, bool> found ;
        private void DFS(Vertex<T> v, ref List<string> list) //使用递归进行深度优先遍历
        {
            list.Add(v.data.ToString());
            string[] org = (string[])list.ToArray().Clone();

            Node node = v.firstEdge;

            while (node != null) //访问此顶点的所有邻接点
            {
                v.visited = true;

                //如果邻接点未被访问，则递归访问它的边
                if (!node.adjvex.visited)
                {
                    DFS(node.adjvex, ref list); //递归
                }
                node = node.next; //访问下一个邻接点
                list = ((string[])org.Clone()).ToList();

                v.visited = false; //将访问标志设为true
            }
            if (v.firstEdge == null && list.Count>1)
            {
                Console.WriteLine("" + "\r\n------------------------\r\n@@@@=" + string.Join(",", list.ToArray()) + "\r\n-----------------------------------");
            }
        }

        private void InitVisited() //初始化visited标志
        {
            foreach (Vertex<T> v in items)
            {
                v.visited = false; //全部置为false
            }
        }

        /// <summary>
        /// 表示链表中的表节点
        /// </summary>
        public class Node
        {
            public Vertex<T> adjvex; //邻接点域
            public Node next; //下一个邻接点指针域
            public Node(Vertex<T> value)
            {
                adjvex = value;
            }
        }
        /// <summary>
        /// 表示存放于数组中的表头节点
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        public class Vertex<TValue>
        {
            public TValue data; //数据
            public Node firstEdge; //邻接点链表头指针
            public Boolean visited; //访问标志,遍历时使用
            public Vertex(TValue value) //构造方法
            {
                data = value;
            }
        }
    }
}
