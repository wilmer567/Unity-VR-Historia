#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class EjecutarTodoSyntro
{
    private static readonly string TRIGGER = Path.Combine(Application.dataPath, "..", "Temp", "Syntro_EJECUTAR_TODO.trigger");

    static EjecutarTodoSyntro()
    {
        if (!File.Exists(TRIGGER)) return;

        File.Delete(TRIGGER);
        EditorApplication.delayCall += EjecutarSecuencia;
    }

    static void EjecutarSecuencia()
    {
        Debug.Log("========================================================");
        Debug.Log("[Syntro] EJECUCION AUTOMATICA: Iniciando configuracion completa...");
        Debug.Log("========================================================");

        try
        {
            Debug.Log("[Syntro] >>> PASO 1/3: Generando texturas PBR y aplicando a materiales...");
            AplicarTexturasPBR.Ejecutar();
            Debug.Log("[Syntro] >>> PASO 1/3 COMPLETADO");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Syntro] Error en Paso 1 (PBR): " + e.Message + "\n" + e.StackTrace);
        }

        try
        {
            Debug.Log("[Syntro] >>> PASO 2/3: Configurando graficos fotorrealistas (materiales, luces, fog, post-processing)...");
            ConfiguradorGraficosRealistas.Ejecutar();
            Debug.Log("[Syntro] >>> PASO 2/3 COMPLETADO");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Syntro] Error en Paso 2 (Graficos): " + e.Message + "\n" + e.StackTrace);
        }

        try
        {
            Debug.Log("[Syntro] >>> PASO 3/4: Configurando cuadros fotorrealistas con portal...");
            ConfiguradorCuadrosFotorrealistas.Ejecutar();
            Debug.Log("[Syntro] >>> PASO 3/4 COMPLETADO");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Syntro] Error en Paso 3 (Cuadros): " + e.Message + "\n" + e.StackTrace);
        }

        try
        {
            Debug.Log("[Syntro] >>> PASO 4/5: Rediseñando Sala de Servidores...");
            DisenoSalaServidores.Ejecutar();
            Debug.Log("[Syntro] >>> PASO 4/5 COMPLETADO");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Syntro] Error en Paso 4 (Sala Servidores): " + e.Message + "\n" + e.StackTrace);
        }

        try
        {
            Debug.Log("[Syntro] >>> PASO 5/6: Construyendo Edificio Syntro completo...");
            EdificioSyntro.Ejecutar();
            Debug.Log("[Syntro] >>> PASO 5/6 COMPLETADO");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Syntro] Error en Paso 5 (Edificio): " + e.Message + "\n" + e.StackTrace);
        }

        try
        {
            Debug.Log("[Syntro] >>> PASO 6/7: Expandiendo y aterrorizando Sala de Servidores...");
            SalaServidoresTerror.Ejecutar();
            Debug.Log("[Syntro] >>> PASO 6/7 COMPLETADO");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Syntro] Error en Paso 6 (Terror Servidores): " + e.Message + "\n" + e.StackTrace);
        }

        try
        {
            Debug.Log("[Syntro] >>> PASO 7/7: Construyendo Laboratorio Piso 3 Terror...");
            LaboratorioPiso3Terror.Ejecutar();
            Debug.Log("[Syntro] >>> PASO 7/7 COMPLETADO");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Syntro] Error en Paso 7 (Lab Piso 3): " + e.Message + "\n" + e.StackTrace);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("========================================================");
        Debug.Log("[Syntro] EJECUCION COMPLETA: Todo configurado.");
        Debug.Log("  - 9 texturas PBR (Color+Normal+Roughness) generadas");
        Debug.Log("  - 3 materiales de escena (Piso, Pared, Rack) con PBR");
        Debug.Log("  - 1 material marco madera/vidrio");
        Debug.Log("  - Materiales asignados a Piso1, Gradas y Piso3");
        Debug.Log("  - 3 Reflection Probes configurados");
        Debug.Log("  - Niebla exponencial oscura activada");
        Debug.Log("  - Global Volume: Bloom, Vignette, ACES, Film Grain");
        Debug.Log("  - Post-processing activado en Main Camera");
        Debug.Log("  - 6 fotos procedurales de Syntro generadas");
        Debug.Log("  - 6 materiales de cuadro con shader mezcla foto/portal");
        Debug.Log("  - 6 portales configurados con mezcla por proximidad");
        Debug.Log("  - Sala de Servidores rediseñada con +40 elementos");
        Debug.Log("  - Edificio Syntro completo: pasillos, 28+ puertas decorativas, fachada");
        Debug.Log("  - Laboratorio Piso 3: terror, colisiones, 11 estaciones, vigilancia");
        Debug.Log("========================================================");
        Debug.Log("[Syntro] >>> Guarda la escena (Ctrl+S) para conservar los cambios.");
    }
}
#endif
