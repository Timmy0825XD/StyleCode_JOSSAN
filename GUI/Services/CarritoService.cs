using ENTITY.Pedidos;

namespace GUI.Services
{
    public class CarritoService
    {
        private List<CarritoItemDTO> _items = new();

        public event Action? OnChange;

        public void AgregarItem(CarritoItemDTO item)
        {
            var itemExistente = _items.FirstOrDefault(i => i.IdVariante == item.IdVariante);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += item.Cantidad;

                if (itemExistente.Cantidad > itemExistente.StockDisponible)
                {
                    itemExistente.Cantidad = itemExistente.StockDisponible;
                }
            }
            else
            {
                _items.Add(item);
            }

            NotificarCambio();
        }

        public void ActualizarCantidad(int idVariante, int nuevaCantidad)
        {
            var item = _items.FirstOrDefault(i => i.IdVariante == idVariante);

            if (item != null)
            {
                if (nuevaCantidad <= 0)
                {
                    EliminarItem(idVariante);
                    return;
                }

                if (nuevaCantidad > item.StockDisponible)
                {
                    nuevaCantidad = item.StockDisponible;
                }

                if (nuevaCantidad > 99)
                {
                    nuevaCantidad = 99;
                }

                item.Cantidad = nuevaCantidad;
                NotificarCambio();
            }
        }

        public void EliminarItem(int idVariante)
        {
            _items.RemoveAll(i => i.IdVariante == idVariante);
            NotificarCambio();
        }

        public void Limpiar()
        {
            _items.Clear();
            NotificarCambio();
        }

        public List<CarritoItemDTO> ObtenerItems()
        {
            return _items.ToList();
        }

        public int ObtenerCantidadTotal()
        {
            return _items.Sum(i => i.Cantidad);
        }

        public decimal ObtenerTotal()
        {
            // El total es la suma directa de todos los subtotales
            // Los precios ya incluyen IVA, no lo mostramos por separado al cliente
            return _items.Sum(i => i.Subtotal);
        }

        public bool EstaVacio()
        {
            return !_items.Any();
        }

        public int ObtenerCantidadProducto(int idVariante)
        {
            var item = _items.FirstOrDefault(i => i.IdVariante == idVariante);
            return item?.Cantidad ?? 0;
        }

        public bool ContieneProducto(int idVariante)
        {
            return _items.Any(i => i.IdVariante == idVariante);
        }

        private void NotificarCambio()
        {
            OnChange?.Invoke();
        }
    }
}
