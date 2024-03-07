using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jane.Common.Utility.Tree
{
    /// <summary>
    /// 二叉树定义
    /// </summary>
    public class TreeNode
    {
        public int data;
        public TreeNode leftChild;
        public TreeNode rightChild;

        public TreeNode(int val)
        {
            this.data = val;
        }

        public TreeNode(int val = 0, TreeNode left = null, TreeNode right = null)
        {
            this.data = val;
            this.leftChild = left;
            this.rightChild = right;
        }
    }
}
