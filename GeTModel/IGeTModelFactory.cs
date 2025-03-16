using System;
using System.Collections.Generic;
using System.Text;

namespace GeTPlanModel
{

    public interface IGeTModelFactory<TModel, TProto>
    {
        public TModel FromProto(TProto proto);
    }

    public interface IProtoFactory<TModel, TProto>
    {
        public TProto ToProto(TModel model);
    }
}
