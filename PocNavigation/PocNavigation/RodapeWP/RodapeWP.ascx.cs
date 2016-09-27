using System;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint.Client;
using ClientTaxonomy = Microsoft.SharePoint.Client.Taxonomy;
using Microsoft.SharePoint;
using System.Web.UI.WebControls;
using System.Net;
using Microsoft.SharePoint.Client.Publishing.Navigation;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Linq;

namespace PocNavigation.RodapeWP
{
    [ToolboxItemAttribute(false)]
    public partial class RodapeWP : WebPart
    {
        private DataList dataList = new DataList();
        private Label errorMessage = new Label();
        private Label grupoTermoStore = new Label();

        private IList<Item> Rodape { get; set; }
        
        // Uncomment the following SecurityPermission attribute only when doing Performance Profiling on a farm solution
        // using the Instrumentation method, and then remove the SecurityPermission attribute when the code is ready
        // for production. Because the SecurityPermission attribute bypasses the security check for callers of
        // your constructor, it's not recommended for production purposes.
        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, UnmanagedCode = true)]
        public RodapeWP()
        {

        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeControl();
        }

        protected void Page_Load(object sender, EventArgs e)
        {


        }

        //Pode ser colcoado como um Serviço, melhoria de código a ser reavaliada
        private void SolicitarDados()
        {
            Rodape = new List<Item>();

            var contexto = new ClientContext(SPContext.Current.Site.Url);
            contexto.Credentials = new NetworkCredential("spadmin", "P@ssw0rd", "sharepoint");

            ClientTaxonomy.TaxonomySession taxonomySession = ClientTaxonomy.TaxonomySession.GetTaxonomySession(contexto);
            taxonomySession.UpdateCache();

            contexto.Load(taxonomySession, ts => ts.TermStores);
            contexto.ExecuteQuery();

            if (taxonomySession.TermStores.Count == 0)
                throw new InvalidOperationException("The Taxonomy Service is offline or missing");

            ClientTaxonomy.TermStore termStore = taxonomySession.TermStores[0];
            var termos = termStore.GetTermSet(new Guid("81a3f867-c153-4c5d-afbc-798a7a5e1df8"));

            var navigation = NavigationTermSet.GetAsResolvedByWeb(contexto, termos, contexto.Web, "GlobalNavigationTaxonomyProvider");
            contexto.Load(navigation, n => n.IsNavigationTermSet, n => n.Terms.IncludeWithDefaultProperties(x => x.Title));
            contexto.ExecuteQuery();

            if (!navigation.IsNavigationTermSet)
                throw new InvalidOperationException("Não é um navigation property");

            //Recursividade pode ser necessário
            foreach (var navItem in navigation.Terms)
            {
                var titulo = navItem.Title;
                var url = navItem.GetWebRelativeFriendlyUrl();
                var itemPai = new Item(titulo.Value, url.Value);

                if (navItem.Terms.Count > 0)
                {
                    foreach (var item in navItem.Terms)
                    {
                        var friendlyUrl = item.GetWebRelativeFriendlyUrl();
                        contexto.ExecuteQuery();
                        itemPai.AdicionarItensFilhos(new Item(item.Title.Value, item.GetWebRelativeFriendlyUrl().Value));
                    }
                }

                Rodape.Add(itemPai);
            }
        }

        protected override void CreateChildControls()
        {
            var htmlGenerico = ControleGenerico();

            Controls.Add(htmlGenerico);
            Controls.Add(errorMessage);
            base.CreateChildControls();
        }

        private HtmlGenericControl ControleGenerico()
        {
            try
            {
                SolicitarDados();
                if (Rodape.Count == 0)
                    throw new InvalidOperationException("Não há dados");
                var todoItens = Rodape.GetEnumerator();
                var ul = new HtmlGenericControl("ul");

                while (todoItens.MoveNext())
                {
                    var itemAtual = todoItens.Current;
                    var li = new HtmlGenericControl("li");
                    li.InnerText = itemAtual.Titulo;
                    ul.Controls.Add(li);

                    var itenFilhos = itemAtual.ItensFilhos.GetEnumerator();

                    while (itenFilhos.MoveNext())
                    {
                        var itemFilhoAtual = itenFilhos.Current;
                        var ulFilho = new HtmlGenericControl("ul");
                       ul.Controls.Add(AddItemFilho(itemFilhoAtual, ul));
                    }
                }
                return ul;
            }
            catch (Exception ex)
            {
                errorMessage.Text = ex.Message;
                var label = new HtmlGenericControl();
                label.InnerText = errorMessage.Text;
                return label;
            }
        }

        private HtmlGenericControl AddItemFilho(Item itemFilho, HtmlGenericControl pLi)
        {
            var li = new HtmlGenericControl("li");
            li.InnerText = itemFilho.Titulo;
            pLi.Controls.Add(li);
            return pLi;
        }
    }
}
