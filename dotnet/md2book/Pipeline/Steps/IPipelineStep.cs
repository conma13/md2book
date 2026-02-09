using System;
using System.Collections.Generic;
using System.Text;

namespace md2book.Pipeline.Steps
{
    public interface IPipelineStep<TContext>
    {
        void Execute(TContext context);
    }
}
