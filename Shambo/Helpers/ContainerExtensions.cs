﻿using Autofac;
using Shambo.Services;

namespace Shambo.Helpers
{
   public static class ContainerExtensions
   {
      public static IDataService GetDataService(this IContainer container)
      {
         return container.Resolve<IDataService>();
      }
   }
}