using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadGcode
{
	public class TreeNode
	{
		public UserEntity Data { get; set; } = null;
		public TreeNode Left { get; set; } = null;
		public TreeNode Right { get; set; } = null;
		public TreeNode(UserEntity data)
		{
			Data = data;
		}

		public UserEntity Insert(UserEntity data)
		{
			if (data.Properties.Order < Data.Properties.Order)
			{
				if (Left == null)
					Left = new TreeNode(data);
				else
					Left.Insert(data);
			}

			if (data.Properties.Order > Data.Properties.Order)
			{
				if (Right == null)
					Right = new TreeNode(data);
				else
					Right.Insert(data);
			}

			return data;
		}

		public List<UserEntity> ToSortList(List<UserEntity> list = null)
		{
			if (list == null) 
				list = new List<UserEntity>();

			if (Left != null)
				Left.ToSortList(list);

			list.Add(Data);

			if (Right != null)
				Right.ToSortList(list);

			return list;
		}

		public static TreeNode TreeFromList(List<UserEntity> list)
		{
			if (list.Count > 0) { 
				TreeNode tree = new TreeNode(list[0]);
				for (int i = 1; i < list.Count; i++)
					tree.Insert(list[i]);
				
				return tree;
			}
			return null;
		}
	}
}
