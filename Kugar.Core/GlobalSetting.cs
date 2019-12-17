using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;

namespace Kugar.Core
{
    public static class KugarGlobalSetting
    {
        private static CompositionContainer _mefContainer=null;


        static KugarGlobalSetting()
        { 
            //_mefContainer=new CompositionContainer();

            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory));

            _mefContainer = new CompositionContainer(catalog);
        }


        public static CompositionContainer MEFCompositionContainer { get { return _mefContainer; } }
    }
}
