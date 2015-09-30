﻿using System.Collections.Generic;
using System.Linq;

namespace kasthack.yandex.pdd {
    public abstract class RawMethodsBase {
        protected internal readonly DomainRawContext Context;

        protected internal IEnumerable<KeyValuePair<string, string>> EmptyParams => Enumerable.Empty<KeyValuePair<string, string>>();

        internal RawMethodsBase( DomainRawContext context ) { Context = context; }
    }
}