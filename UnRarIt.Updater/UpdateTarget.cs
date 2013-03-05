using System;
using System.ComponentModel;

namespace UnRarIt.Updater
{
  public interface IUpdateTarget : ISynchronizeInvoke
  {
    Version AppVersion { get; }
    Uri UpdateUri { get; }
  }
}
