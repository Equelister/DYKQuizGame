using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYKShared.Model
{
    public class CategoryModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Descritpion { get; set; }


        public CategoryModel(int id, string name, string description)
        {
            ID = id;
            Name = name;
            Descritpion = description;
        }
        public CategoryModel()
        {

        }
    }

}
