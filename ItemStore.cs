using System;

namespace DynAjax.Communication
{
  public class ItemStore<TK, TT>
  {
    public int LifeSpan { get; set; }
    public TK ItemKey { get; set; }
    public TT Item { get; set; }
    public DateTime LastModified { get; set; }
    public bool Expired => DateTime.Now > LastModified.AddSeconds(LifeSpan);
  }
}
