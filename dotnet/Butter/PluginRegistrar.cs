using Butter.Bindings;

namespace Butter;

public class PluginRegistrar : IDisposable
{
  private readonly PluginRegistrarHandle _handle;

  private readonly Dictionary<Type, IPlugin> _plugins = new();

  internal PluginRegistrar(
    PluginRegistrarHandle handle,
    BinaryMessenger messenger)
  {
    _handle = handle;
    Messenger = messenger;
  }

  public BinaryMessenger Messenger { get; private set; }
  // TextureRegistrar TextureRegistrar { get; }

  public void AddPlugin<T>(IPlugin plugin) where T : IPlugin
  {
    var type = typeof(T);
    if (_plugins.ContainsKey(type))
    {
      throw new ButterException($"Plugin type {type} is already registered.");
    }

    _plugins[type] = plugin;
  }

  public View? GetViewById(int it)
  {
    return null;
  }

  public void Dispose()
  {
    foreach (var plugin in _plugins.Values)
    {
      plugin.Dispose();
    }

    Messenger.Dispose();
    _handle.Dispose();
  }
}

public interface IPlugin : IDisposable { }
