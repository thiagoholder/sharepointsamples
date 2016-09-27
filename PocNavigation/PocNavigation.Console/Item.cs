using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocNavigation.Integracao
{
    public class Item
    {
        private List<Item> itensFilhos;

        public string Titulo { get; private set; }
        public string URL { get; private set; }

        public IEnumerable<Item> ItensFilhos { get { return itensFilhos; } }

        public Item(string titulo, string url)
        {
            Titulo = titulo;
            URL = url;
            itensFilhos = new List<Item>();
        }

        public void AdicionarItensFilhos(Item item)
        {
            itensFilhos.Add(item);
        }




        



    }
}
