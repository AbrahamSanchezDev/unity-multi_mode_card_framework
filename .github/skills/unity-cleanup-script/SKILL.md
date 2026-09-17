---
name: unity-cleanup-script
description: 'Refactoriza scripts C# de Unity a un estilo limpio, legible y conforme a principios SOLID'
argument-hint: 'Provide scriptPath (optional), dryRun (boolean), namespace (optional), or format (default/dotnet)'
user-invocable: true
---
# Skill: clean-script

## Resumen
Skill para refactorizar scripts C# de Unity a un estilo limpio, legible y conforme a principios SOLID. Está diseñada para ejecutarse desde Copilot/agent y aceptar como entrada la ruta de un script C# o el contenido del editor activo.

## Disparadores
- Comando: `clean-script`
- Alias: `refactor-unity-script`, `unity-clean-script`

## Alcance y propósito
- Reorganiza `namespace`, variables, atributos de Unity (`[SerializeField]`, etc.), y métodos según el ciclo de vida de Unity.
- Aplica principios SOLID básicos y extrae helpers si una función hace demasiado.
- Añade documentación XML mínima (`///`) en métodos públicos y bloques lógicos complejos.

## Entradas
- `scriptPath` (opcional): Ruta al archivo C# en el workspace. Si no se provee, la skill intentará usar la ventana del editor activa.
- `dryRun` (opcional, boolean): Si es `true`, devuelve un diff/preview en lugar de sobrescribir.
- `namespace` (opcional): Forzar namespace de salida. Si no se provee, infiere del path o deja el namespace original.
- `format` (opcional): `default` (mantener estilo existente) o `dotnet` (aplicar estilo .editorconfig / dotnet). Default: `default`.

## Salida
- Archivo C# refactorizado (sobrescribe por defecto) o un diff en `dryRun=true`.
- Un breve reporte JSON con los cambios aplicados y puntos de atención (métodos fragmentados, sugerencias manuales).

## Pasos de ejecución
1. Resolver entrada: `scriptPath` o contenido del editor activo.
2. Parseo sintáctico (análisis básico): identificar `namespace`, `using`, clase(s), campos y métodos.
3. Detectar reglas violadas por las pautas del skill (orden de variables, métodos fuera de lugar, métodos largos, campos sin atributos, etc.).
4. Reorganizar el archivo aplicando las siguientes reglas:
   - Mantener `using` agrupados y ordenados.
   - Asegurar `namespace` coherente.
   - Variables agrupadas y ordenadas por modificador de acceso y tipo; `[SerializeField]` justo después de las públicas.
   - Métodos ordenados: Unity lifecycle → Public API → Protected → Private helpers.
   - Extraer responsabilidades en métodos privados descriptivos si el método hace >1 tarea lógica.
   - Añadir `///` a métodos públicos y a bloques complejos.
5. Generar salida: archivo modificado o diff.
6. Devolver `report.json` con resumen de cambios.

## Criterios de calidad
- Ningún método público debe superar ~50 líneas sin extraer responsabilidades (heurística).
- Variables expuestas al inspector deben conservar atributos. Si hay ambigüedad, la skill no elimina atributos.
- No se realizan cambios semánticos (lógica), salvo renombres no ambiguos y reorganización.

## Ejemplos de uso (prompts)
- "Refactoriza el script abierto con clean-script"
- "run clean-script on Assets/Scripts/PlayerController.cs with dryRun=true"
- "clean-script scriptPath=Assets/Scripts/EnemyAI.cs namespace=Game.AI format=dotnet"

## Integración con VS Code tasks
Se puede añadir una tarea que ejecute la skill a través de la CLI del agente o un script PowerShell. Ejemplo de `tasks.json` (snippet):

{
  "label": "clean-script: refactorizar script activo",
  "type": "shell",
  "command": "pwsh -File .vscode/scripts/run-clean-script.ps1 -scriptPath \"${file}\"",
  "problemMatcher": []
}

## Limitaciones
- No ejecuta compilación ni tests; el usuario debe validar el resultado en Unity.
- No reescribe lógica compleja ni repara dependencias rotas.
- El análisis es heurístico: en casos ambiguos, prioriza no romper el código y devuelve sugerencias.

## Buenas prácticas recomendadas
- Hacer `dryRun=true` antes de sobrescribir en archivos críticos.
- Ejecutar tests/unit en proyectos que los tengan tras aplicar refactor.
- Revisar manualmente renombres de métodos públicos usados en otros scripts.

## Versionado y autor
- Versión: 0.1
- Autor: Equipo de desarrollo / Copilot skill

## Outputs programáticos
- `changed`: boolean
- `diff`: string (cuando `dryRun=true`)
- `modifiedPath`: string
- `report`: { filesChanged: number, suggestions: string[] }

## Permisos
- Lectura/escritura en el workspace para `scriptPath` objetivo.

## Próximas mejoras propuestas
- Integración con roslyn para refactorings semánticos más seguros.
- Soporte para reglas personalizadas desde un archivo `.clean-scriptrc`.

---

Si quieres, puedo crear además el script wrapper ` .vscode/scripts/run-clean-script.ps1` y el snippet `tasks.json` para que lo ejecutes desde VS Code. Dime si lo genero ahora.