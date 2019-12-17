using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Collections
{
    public class CategoryTree
    {
        private ITree_DataProvider _provider = null;
        private string _moduleID;
        private BaseStruct.Tree _tree = new Tree(null);

        public CategoryTree(ITree_DataProvider provider, string moduleID)
        {
            _provider = provider;
            _moduleID = moduleID;

            ReLoad();
        }

        public void ReLoad()
        {
            if (_tree != null)
            {
                _tree.Clear();
            }

            var itemList = _provider.GetCategoryList(_moduleID);

            var root = new CategoryItem();
            root.Key = null;

            buildTreeNode(itemList, root);

            _tree.Nodes.AddRange(root.Nodes);
        }



        public IList<CategoryItem> GetChildCategoryByPath(string path)
        {
            return null;
        }

        private void buildTreeNode(IList<CategoryItemForStorage> categoryItems, CategoryItem parent)
        {
            var childList = categoryItems.Where(x => x.ParentKey == parent.Key).ToArray();

            if (childList != null && childList.Length > 0)
            {
                parent.Nodes.AddRange(childList.Cast(x => (CategoryItem)x));

                foreach (var item in childList)
                {
                    buildTreeNode(categoryItems, item);
                }
            }

        }

    }

    public class CategoryItem : BaseStruct.TreeNode
    {
        public static implicit operator CategoryItem(CategoryItemForStorage item)
        {
            var category = new CategoryItem();

            category.Key = item.Key;
            category.Text = item.Text;
            category.Value = item.Value.DeserializeToObject(null);

            return category;
        }
    }






}
