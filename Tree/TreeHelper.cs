using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jane.Common.Utility.Tree
{
    public class TreeHelper
    {
        public static TreeNode CreateNode(int? val)
        {
            if (val == null) return null;
            return new TreeNode((int)val);
        }
        public static TreeNode CreateBinaryTree2(int?[] arr)
        {
            if (arr.Length <= 0 || arr[0] == null)
                return null;
            TreeNode root = TreeHelper.CreateNode(arr[0]);
            Queue<TreeNode> queue = new Queue<TreeNode>();
            queue.Enqueue(root);
            int index = 1;
            while (queue.Count > 0)
            {
                TreeNode node = queue.Dequeue();
                if (node == null) continue;
                if (index < arr.Length)
                {
                    node.leftChild = TreeHelper.CreateNode(arr[index++]);
                    queue.Enqueue(node.leftChild);
                }
                if (index < arr.Length)
                {
                    node.rightChild = TreeHelper.CreateNode(arr[index++]);
                    queue.Enqueue(node.rightChild);
                }
            }
            return root;
        }

        /// <summary>
        /// 二叉树前序遍历
        /// </summary>
        public void PreOrderTraveral(TreeNode node)
        {
            if (node == null)
            {
                return;
            }
            Console.WriteLine(node.data);
            PreOrderTraveral(node.leftChild);
            PreOrderTraveral(node.rightChild);
        }
        /// <summary>
        /// 中序遍历
        /// </summary>
        public void InOrderTraveral(TreeNode node)
        {
            if (node == null)
            {
                return;
            }
            InOrderTraveral(node.leftChild);
            Console.WriteLine(node.data);
            InOrderTraveral(node.rightChild);
        }
        /// <summary>
        /// 后序遍历
        /// </summary>
        public void PostOrderTraveral(TreeNode node)
        {
            if (node == null)
            {
                return;
            }
            PostOrderTraveral(node.leftChild);
            PostOrderTraveral(node.rightChild);
            Console.WriteLine(node.data);
        }
        /// <summary>
        /// 二叉树非递归前序遍历
        /// </summary>
        public void PreOrderTraveralWithStack(TreeNode root)
        {
            Stack<TreeNode> stack = new Stack<TreeNode>();
            TreeNode treeNode = root;
            while (treeNode != null || stack.Count > 0)
            {
                // 迭代访问节点的左孩子，并入栈
                while (treeNode != null)
                {
                    Console.WriteLine(treeNode.data);
                    stack.Push(treeNode);
                    treeNode = treeNode.leftChild;
                }
                // 如果节点没有左孩子，则弹出栈顶节点，访问节点右孩子
                if (stack.Count > 0)
                {
                    treeNode = stack.Pop();
                    treeNode = treeNode.rightChild;
                }
            }
        }
        public void LevelOrderTraversal(TreeNode root)
        {
            Queue<TreeNode> queue = new Queue<TreeNode>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                TreeNode node = queue.Dequeue();
                Console.WriteLine(node.data);
                if (node.leftChild != null)
                {
                    queue.Enqueue(node.leftChild);
                }
                if (node.rightChild != null)
                {
                    queue.Enqueue(node.rightChild);
                }
            }
        }

    }
}
