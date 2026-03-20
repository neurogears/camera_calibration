using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Bonsai.Vision;

[Combinator]
[Description("")]
[WorkflowElementCategory(ElementCategory.Transform)]
public class AppendList
{
    public IObservable<T[]> Process<T>(IObservable<T> source)
    {
        return Observable.Defer(() =>
        {
            var accumulator = new List<T>();
            return source.Select(value =>
            {
                accumulator.Add(value);
                return accumulator.ToArray();
            });
        });
    }
}
