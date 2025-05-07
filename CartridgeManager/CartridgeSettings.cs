namespace Munglo.WeaponsSystem.Cartridges;
class CartridgeSettings
{
    public string GetFullTypeName()
    {
        if (string.IsNullOrWhiteSpace(GetType().Namespace))
        {
            return GetType().Name;
        }
        return $"{GetType().Namespace}.{GetType().Name}";
    }
}// EOF CLASS