# HG.CFDI.API

Este proyecto expone varios endpoints para el timbrado unificado de facturas/Carta Porte.

## Endpoint `TimbraRemision`

```
POST /api/Cfdi/TimbraRemision?database={bd}&remision={remision}&sistemaTimbrado={1|2|3}
```

- **database**: Base de datos donde se localiza la remisión.
- **remision**: Número de guía o remisión.
- **sistemaTimbrado** (opcional, por defecto 2): Permite indicar qué PAC utilizar.
  - `1` = LIS
  - `2` = BuzónE
  - `3` = InvoiceOne

Si se proporciona `sistemaTimbrado` con un valor válido (1..3) éste se asignará
al modelo y se utilizará para seleccionar el PAC.

Al invocar este endpoint, la API intentará actualizar la guía a estatus **En proceso de timbrado**. Si la operación no afecta registros, se responde con `409 Conflict` indicando que la guía ya está en proceso o timbrada.
