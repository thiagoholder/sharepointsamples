using Microsoft.SharePoint.Client;
using ClientTaxonomy = Microsoft.SharePoint.Client.Taxonomy;
using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.SharePoint.Client.Publishing.Navigation;

namespace PocNavigation.Integracao
{
    class Program
    {
        private static IList<Item> Rodape { get; set; }
        

        static void Main(string[] args)
        {
            Console.WriteLine("Aguardando Dados..");
            SolicitarDados();

            var todosIntens = Rodape.GetEnumerator();

            while (todosIntens.MoveNext())
            {


                var itemAtual = todosIntens.Current;

                Console.WriteLine(string.Format("- {0}", itemAtual.Titulo));
                Console.WriteLine(string.Format("- {0}", itemAtual.URL));

                var itenFilhos = itemAtual.ItensFilhos.GetEnumerator();

                while (itenFilhos.MoveNext())
                {
                    var itemFilhoAtual = itenFilhos.Current;


                    Console.WriteLine (string.Format("-- {0}", itemFilhoAtual.Titulo));
                    Console.WriteLine(string.Format("-- {0}", itemFilhoAtual.URL));
                }

                Console.WriteLine();
            }

            Console.ReadKey();

        }


        private static void SolicitarDados()
        {
            Rodape = new List<Item>();

            var contexto = new ClientContext("http://spfarm");
            contexto.Credentials = new NetworkCredential("spadmin", "P@ssw0rd", "sharepoint");
            
            ClientTaxonomy.TaxonomySession taxonomySession = ClientTaxonomy.TaxonomySession.GetTaxonomySession(contexto);
            taxonomySession.UpdateCache();

            contexto.Load(taxonomySession, ts => ts.TermStores);
            contexto.ExecuteQuery();

            if (taxonomySession.TermStores.Count == 0)
                throw new InvalidOperationException("O Servico de Taxonomia estar fora do Ar");

            ClientTaxonomy.TermStore termStore = taxonomySession.TermStores[0];
            var termos = termStore.GetTermSet(new Guid("81a3f867-c153-4c5d-afbc-798a7a5e1df8"));
        
            var navigation = NavigationTermSet.GetAsResolvedByWeb(contexto, termos, contexto.Web, "GlobalNavigationTaxonomyProvider");
           
            contexto.Load(navigation, n => n.IsNavigationTermSet, n => n.Terms.IncludeWithDefaultProperties(x => x.Title));
            contexto.ExecuteQuery();

            if (!navigation.IsNavigationTermSet)
                throw new InvalidOperationException("Não é um navigation property");

            var itensNavegacao = navigation.Terms.GetEnumerator();

            //Recursividade pode ser necessário
            while (itensNavegacao.MoveNext())
            {
                var itemAtual = itensNavegacao.Current;
                var titulo = itemAtual.Title;
                var url = itemAtual.GetWebRelativeFriendlyUrl();
                contexto.Load(itemAtual, x => x.Terms.IncludeWithDefaultProperties(i => i.Title));
                contexto.ExecuteQuery();
                
                var itemPai = new Item(titulo.Value, url.Value);
                var itemFilho = itemAtual.Terms.GetEnumerator();

                while (itemFilho.MoveNext())
                {
                    var itemFilhoAtual = itemFilho.Current;
                    var friendlyUrl = itemFilhoAtual.GetWebRelativeFriendlyUrl();
                    contexto.ExecuteQuery();
                    itemPai.AdicionarItensFilhos(new Item(itemFilhoAtual.Title.Value, friendlyUrl.Value));

                }

                Rodape.Add(itemPai);
            }
            
        }

    }

}

