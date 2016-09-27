using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace PocNavigation.Testes
{
    [TestClass]
    public class UnitTest1
    {
        
        [TestMethod]
        public void DeveraEncapsularUmaColecao()
        {
            var item = new Item("Pai", @"http://www.google.com.br");

            item.AdicionarItensFilhos(new Item("Filho A", @"http://www.filhoa.com.br"));
            item.AdicionarItensFilhos(new Item("Filho B", @"http://www.filhob.com.br"));
            item.AdicionarItensFilhos(new Item("Filho C", @"http://www.filhoc.com.br"));
            item.AdicionarItensFilhos(new Item("Filho D", @"http://www.filhod.com.br"));
            item.AdicionarItensFilhos(new Item("Filho E", @"http://www.filhoe.com.br"));

            Assert.AreEqual(item.ItensFilhos.ToList().Count, 5);

        }
    }
}
