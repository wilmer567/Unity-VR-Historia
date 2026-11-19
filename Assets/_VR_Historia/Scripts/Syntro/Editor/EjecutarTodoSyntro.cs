#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class EjecutarTodoSyntro
{
    static EjecutarTodoSyntro()
    {
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
            Debug.Log("[Syntro] >>> PASO 7/8: Construyendo Laboratorio Piso 3 Terror...");
            LaboratorioPiso3Terror.Ejecutar();
            Debug.Log("[Syntro] >>> PASO 7/8 COMPLETADO");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Syntro] Error en Paso 7 (Lab Piso 3): " + e.Message + "\n" + e.StackTrace);
        }

        try
        {
            Debug.Log("[Syntro] >>> PASO 8/8: Mejorando corredor, HUD e iluminación terror...");
            SyntroCorredorEnhancer.MejorarCorredor();
            Debug.Log("[Syntro] >>> PASO 8/8 COMPLETADO");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Syntro] Error en Paso 8 (Corredor Enhancer): " + e.Message + "\n" + e.StackTrace);
        }

        AssetDatabase.SaveAssets();
        var escenaActual = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(escenaActual);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(escenaActual);
        AssetDatabase.Refresh();

        Debug.Log("========================================================");
        Debug.Log("[Syntro] EJECUCION COMPLETA: Todo configurado y guardado en la escena.");
        Debug.Log("========================================================");
    }
}
#endif
