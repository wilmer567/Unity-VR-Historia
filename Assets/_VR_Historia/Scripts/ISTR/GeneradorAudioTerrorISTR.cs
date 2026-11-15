#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class GeneradorAudioTerrorISTR
{
    const string RUTA = "Assets/_Unity Essentials/Audio/Terror";
    const int SR = 44100;
    static System.Random rng;

    [MenuItem("ISTR Terror/Generar Audio Procedural Terror")]
    public static void Ejecutar()
    {
        rng = new System.Random(666);

        string dir = Application.dataPath + "/_Unity Essentials/Audio/Terror";
        Directory.CreateDirectory(dir);

        Debug.Log("[AudioTerror] Generando 18 clips de audio procedural...");

        Progreso("Alarma servidor...", 0.05f);
        Wav("alarma_servidor", GenAlarmaServidor());

        Progreso("Reinicio exitoso...", 0.1f);
        Wav("reinicio_exitoso", GenReinicioExitoso());

        Progreso("Puerta desbloqueo...", 0.15f);
        Wav("puerta_desbloqueo", GenPuertaDesbloqueo());

        Progreso("Musica gradas tension...", 0.2f);
        Wav("musica_gradas_tension", GenMusicaTension());

        Progreso("Anomalia final...", 0.3f);
        Wav("anomalia_final", GenAnomaliaFinal());

        Progreso("Musica climax...", 0.38f);
        Wav("musica_climax_final", GenMusicaClimax());

        Progreso("Flicker luz...", 0.5f);
        Wav("flicker_luz", GenFlickerLuz());

        Progreso("Susurro cercano...", 0.55f);
        Wav("susurro_cercano", GenSusurroCercano());

        Progreso("Distorsion grave...", 0.6f);
        Wav("distorsion_grave", GenDistorsionGrave());

        Progreso("Pasos fantasma...", 0.65f);
        Wav("paso_fantasma_1", GenPasoFantasma(0));
        Wav("paso_fantasma_2", GenPasoFantasma(1));
        Wav("paso_fantasma_3", GenPasoFantasma(2));

        Progreso("Susurros gradas...", 0.72f);
        Wav("susurro_gradas_1", GenSusurroGradas(0));
        Wav("susurro_gradas_2", GenSusurroGradas(1));

        Progreso("Latido cardiaco...", 0.78f);
        Wav("latido_cardiaco", GenLatidoCardiaco());

        Progreso("Tos...", 0.83f);
        Wav("tos_1", GenTos(0));
        Wav("tos_2", GenTos(1));

        Progreso("Aliento frio...", 0.88f);
        Wav("aliento_frio", GenAlientoFrio());

        Progreso("Ambiente maquinas...", 0.89f);
        Wav("ambiente_maquinas", GenAmbienteMaquinas());

        Progreso("Ambiente gradas...", 0.90f);
        Wav("ambiente_gradas", GenAmbienteGradas());

        Progreso("Ambiente laboratorio...", 0.91f);
        Wav("ambiente_laboratorio", GenAmbienteLaboratorio());

        Progreso("Importando assets...", 0.93f);
        AssetDatabase.Refresh();

        Progreso("Asignando a managers...", 0.96f);
        AsignarAudio();

        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();

        Debug.Log("[AudioTerror] 21 clips generados y asignados a los managers.");
    }

    static void Progreso(string msg, float p)
    {
        EditorUtility.DisplayProgressBar("Audio Terror ISTR", msg, p);
    }

    // ===========================================================
    //  PRIMITIVAS DE SINTESIS
    // ===========================================================

    static float S(float freq, float t)
    {
        return Mathf.Sin(2f * Mathf.PI * freq * t);
    }

    static float Ruido()
    {
        return (float)(rng.NextDouble() * 2.0 - 1.0);
    }

    static float Env(float t, float atk, float sus, float rel)
    {
        if (t < atk) return t / Mathf.Max(atk, 0.001f);
        if (t < atk + sus) return 1f;
        float r = t - atk - sus;
        if (r < rel) return 1f - r / Mathf.Max(rel, 0.001f);
        return 0f;
    }

    static void PasoBajo(float[] s, float cutoff)
    {
        float rc = 1f / (2f * Mathf.PI * cutoff);
        float dt = 1f / SR;
        float a = dt / (rc + dt);
        for (int i = 1; i < s.Length; i++)
            s[i] = s[i - 1] + a * (s[i] - s[i - 1]);
    }

    static void Normalizar(float[] s, float pico = 0.9f)
    {
        float mx = 0f;
        for (int i = 0; i < s.Length; i++)
            if (Mathf.Abs(s[i]) > mx) mx = Mathf.Abs(s[i]);
        if (mx < 0.0001f) return;
        float sc = pico / mx;
        for (int i = 0; i < s.Length; i++) s[i] *= sc;
    }

    static void Reverb(float[] s, float mix)
    {
        int d1 = (int)(0.037f * SR);
        int d2 = (int)(0.059f * SR);
        int d3 = (int)(0.083f * SR);
        float[] o = (float[])s.Clone();
        for (int i = d3; i < s.Length; i++)
            s[i] = o[i] + mix * (0.5f * o[i - d1] + 0.35f * o[i - d2] + 0.2f * o[i - d3]);
    }

    static float[] BandPass(float[] src, float lo, float hi)
    {
        float[] lp = (float[])src.Clone();
        PasoBajo(lp, hi);
        float[] lp2 = (float[])src.Clone();
        PasoBajo(lp2, lo);
        float[] res = new float[src.Length];
        for (int i = 0; i < src.Length; i++)
            res[i] = lp[i] - lp2[i];
        return res;
    }

    // ===========================================================
    //  GENERADORES DE SONIDO
    // ===========================================================

    // --- 1. Alarma de servidor: beep pulsante alternando frecuencias ---
    static float[] GenAlarmaServidor()
    {
        int n = 2 * SR;
        float[] s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float pulso = ((t % 0.5f) < 0.3f) ? 1f : 0f;
            float freq = ((int)(t / 0.5f) % 2 == 0) ? 880f : 660f;
            s[i] = S(freq, t) * 0.6f + S(freq * 2f, t) * 0.15f + Ruido() * 0.04f;
            s[i] *= pulso;
        }
        Normalizar(s, 0.85f);
        return s;
    }

    // --- 2. Reinicio exitoso: tono ascendente ---
    static float[] GenReinicioExitoso()
    {
        int n = (int)(1.5f * SR);
        float[] s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float freq = Mathf.Lerp(300f, 800f, t / 1.5f);
            float env = Env(t, 0.05f, 0.8f, 0.65f);
            s[i] = S(freq, t) * 0.5f + S(freq * 1.5f, t) * 0.2f;
            s[i] *= env;
        }
        Reverb(s, 0.4f);
        Normalizar(s, 0.7f);
        return s;
    }

    // --- 3. Puerta desbloqueo: clic metalico + chirrido ---
    static float[] GenPuertaDesbloqueo()
    {
        int n = SR;
        float[] s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float click = (t < 0.05f) ? Ruido() * (1f - t / 0.05f) * 0.8f : 0f;
            float ring = S(1200f, t) * Mathf.Exp(-t * 8f) * 0.4f
                       + S(2400f, t) * Mathf.Exp(-t * 12f) * 0.15f;
            float creak = 0f;
            if (t > 0.15f && t < 0.8f)
            {
                float ct = t - 0.15f;
                creak = S(180f + 60f * S(3f, ct), ct) * Env(ct, 0.1f, 0.3f, 0.25f) * 0.3f;
            }
            s[i] = click + ring + creak;
        }
        Reverb(s, 0.3f);
        Normalizar(s, 0.8f);
        return s;
    }

    // --- 4. Musica gradas tension (15s loop): drone bajo con armonicos ---
    static float[] GenMusicaTension()
    {
        int n = 15 * SR;
        float[] drone = new float[n];
        float[] alto = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float lfo1 = 0.5f + 0.5f * S(0.13f, t);
            float lfo2 = 0.5f + 0.5f * S(0.07f, t);

            drone[i] = S(55f, t) * 0.35f
                     + S(82.5f, t) * 0.2f * lfo1
                     + S(110f, t) * 0.1f * lfo2
                     + S(36.7f, t) * 0.15f
                     + S(58.27f, t) * 0.08f * lfo2
                     + Ruido() * 0.03f * lfo1;

            float ciclo = t % 5f;
            if (ciclo > 4.5f && ciclo < 4.7f)
                alto[i] = S(440f, t) * Env(ciclo - 4.5f, 0.02f, 0.05f, 0.13f) * 0.15f;
        }
        PasoBajo(drone, 300f);
        for (int i = 0; i < n; i++) drone[i] += alto[i];
        Normalizar(drone, 0.8f);
        return drone;
    }

    // --- 5. Anomalia final (3s): glitch digital ---
    static float[] GenAnomaliaFinal()
    {
        int n = 3 * SR;
        float[] s = new float[n];
        float freq = 200f;
        int salto = SR / 20;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            if (salto > 0 && i % salto == 0)
                freq = 100f + (float)rng.NextDouble() * 2000f;
            float env = Env(t, 0.3f, 2f, 0.7f);
            float sq = (S(freq, t) > 0f) ? 0.4f : -0.4f;
            s[i] = (sq + Ruido() * 0.2f + S(40f, t) * 0.2f) * env;
        }
        Normalizar(s, 0.85f);
        return s;
    }

    // --- 6. Musica climax (20s loop): drone intenso que construye ---
    static float[] GenMusicaClimax()
    {
        int n = 20 * SR;
        float[] s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float p = t / 20f;
            float pulseRate = Mathf.Lerp(1.2f, 2.5f, p);
            float pulse = 0.5f + 0.5f * S(pulseRate, t);

            float bass = S(45f, t) * 0.4f * pulse + S(67.5f, t) * 0.2f;
            float rise = S(Mathf.Lerp(100f, 800f, p), t) * 0.15f * p
                       + S(Mathf.Lerp(200f, 1200f, p), t) * 0.08f * p;
            float dis = S(47.5f, t) * 0.1f * p + S(93f, t) * 0.08f * p;
            float noise = Ruido() * 0.04f * (0.3f + 0.7f * p);

            s[i] = bass + rise + dis + noise;
        }
        PasoBajo(s, 600f);
        Normalizar(s, 0.85f);
        return s;
    }

    // --- 7. Flicker luz (1s): zumbido electrico intermitente ---
    static float[] GenFlickerLuz()
    {
        int n = SR;
        float[] s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float buzz = S(120f, t) * 0.3f + S(240f, t) * 0.15f;
            float flick = (rng.NextDouble() > 0.3) ? 1f : 0f;
            s[i] = (buzz + Ruido() * 0.2f) * flick;
        }
        PasoBajo(s, 2000f);
        Normalizar(s, 0.7f);
        return s;
    }

    // --- 8. Susurro cercano (2s): ruido filtrado con patron de respiracion ---
    static float[] GenSusurroCercano()
    {
        int n = 2 * SR;
        float[] s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float env = Env(t, 0.3f, 1.0f, 0.7f);
            float breath = 0.5f + 0.5f * S(2.5f, t);
            s[i] = Ruido() * env * breath;
        }
        s = BandPass(s, 800f, 3500f);
        Reverb(s, 0.3f);
        Normalizar(s, 0.6f);
        return s;
    }

    // --- 9. Distorsion grave (2s): retumbo de baja frecuencia ---
    static float[] GenDistorsionGrave()
    {
        int n = 2 * SR;
        float[] s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float env = Env(t, 0.2f, 1.2f, 0.6f);
            float mod = 0.5f + 0.5f * S(3f, t);
            s[i] = (S(35f, t) * 0.5f * mod + S(70f, t) * 0.3f + S(52.5f, t) * 0.2f) * env;
            s[i] = Mathf.Clamp(s[i] * 2f, -0.8f, 0.8f);
        }
        Normalizar(s, 0.85f);
        return s;
    }

    // --- 10. Paso fantasma (0.4s): impacto suave y distante ---
    static float[] GenPasoFantasma(int v)
    {
        int n = (int)(0.4f * SR);
        float[] s = new float[n];
        float bf = 80f + v * 30f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float env = Mathf.Exp(-t * 15f);
            s[i] = S(bf, t) * env * 0.5f + Ruido() * Mathf.Exp(-t * 20f) * 0.3f;
        }
        PasoBajo(s, 500f);
        Reverb(s, 0.5f);
        Normalizar(s, 0.6f);
        return s;
    }

    // --- 11. Susurro gradas (1.5s): ruido aereo modulado ---
    static float[] GenSusurroGradas(int v)
    {
        int n = (int)(1.5f * SR);
        float[] s = new float[n];
        float bf = 1.8f + v * 0.5f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            s[i] = Ruido() * Env(t, 0.2f, 0.8f, 0.5f) * (0.5f + 0.5f * S(bf, t));
        }
        s = BandPass(s, 600f, 2500f);
        Reverb(s, 0.4f);
        Normalizar(s, 0.5f);
        return s;
    }

    // --- 12. Latido cardiaco (1.2s loop): doble golpe lub-dub ---
    static float[] GenLatidoCardiaco()
    {
        int n = (int)(1.2f * SR);
        float[] s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            if (t < 0.12f)
                s[i] = S(60f, t) * Mathf.Exp(-t * 20f) * 0.9f
                     + S(40f, t) * Mathf.Exp(-t * 15f) * 0.5f;
            float dt2 = t - 0.25f;
            if (dt2 > 0f && dt2 < 0.1f)
                s[i] += S(80f, dt2) * Mathf.Exp(-dt2 * 25f) * 0.6f
                      + S(50f, dt2) * Mathf.Exp(-dt2 * 18f) * 0.3f;
        }
        PasoBajo(s, 150f);
        Normalizar(s, 0.85f);
        return s;
    }

    // --- 13. Tos (0.6s): explosion de ruido con resonancia ---
    static float[] GenTos(int v)
    {
        int n = (int)(0.6f * SR);
        float[] s = new float[n];
        float delay = 0.02f + v * 0.03f;
        float resFreq = 250f + v * 80f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR - delay;
            if (t < 0f) continue;
            float env = Mathf.Exp(-t * 8f);
            s[i] = Ruido() * env * 0.7f + S(resFreq, t) * env * 0.3f;
        }
        PasoBajo(s, 3500f);
        Normalizar(s, 0.7f);
        return s;
    }

    // --- 14. Aliento frio (2s): exhalacion suave y fantasmal ---
    static float[] GenAlientoFrio()
    {
        int n = 2 * SR;
        float[] s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            s[i] = Ruido() * Env(t, 0.4f, 0.8f, 0.8f);
        }
        s = BandPass(s, 400f, 2000f);
        Reverb(s, 0.3f);
        Normalizar(s, 0.5f);
        return s;
    }

    // --- 15. Ambiente maquinas (30s loop): zumbido constante de sala de servidores ---
    static float[] GenAmbienteMaquinas()
    {
        int n = 30 * SR;
        float[] s = new float[n];
        float nextPing = 8f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float lfo = 0.85f + 0.15f * S(0.05f, t);
            s[i] = (S(60f, t) * 0.25f + S(120f, t) * 0.1f) * lfo;
            s[i] += Ruido() * 0.015f;
            if (t > nextPing && t < nextPing + 0.08f)
            {
                float pt = t - nextPing;
                float pingFreq = 800f + (float)rng.NextDouble() * 1200f;
                s[i] += S(pingFreq, pt) * Mathf.Exp(-pt * 60f) * 0.08f;
            }
            if (t > nextPing + 0.08f)
                nextPing += 6f + (float)rng.NextDouble() * 4f;
        }
        PasoBajo(s, 250f);
        Normalizar(s, 0.5f);
        return s;
    }

    // --- 16. Ambiente gradas (20s loop): eco de escalera con viento ---
    static float[] GenAmbienteGradas()
    {
        int n = 20 * SR;
        float[] s = new float[n];
        float nextMoan = 7f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float breath = 0.4f + 0.6f * (0.5f + 0.5f * S(0.2f, t));
            s[i] = Ruido() * breath;
            if (t > nextMoan && t < nextMoan + 2f)
            {
                float mt = t - nextMoan;
                float moanFreq = Mathf.Lerp(80f, 50f, mt / 2f);
                s[i] += S(moanFreq, mt) * Env(mt, 0.3f, 1f, 0.7f) * 0.06f;
            }
            if (t > nextMoan + 2f)
                nextMoan += 5f + (float)rng.NextDouble() * 4f;
        }
        s = BandPass(s, 200f, 1500f);
        int r1 = (int)(0.10f * SR), r2 = (int)(0.17f * SR), r3 = (int)(0.23f * SR);
        float[] o = (float[])s.Clone();
        for (int i = r3; i < n; i++)
            s[i] = o[i] + 0.35f * (0.5f * o[i - r1] + 0.35f * o[i - r2] + 0.2f * o[i - r3]);
        Normalizar(s, 0.45f);
        return s;
    }

    // --- 17. Ambiente laboratorio (25s loop): electronico esteril ---
    static float[] GenAmbienteLaboratorio()
    {
        int n = 25 * SR;
        float[] s = new float[n];
        float nextBeep = 6f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float lfo = 0.7f + 0.3f * S(0.1f, t);
            s[i] = S(50f, t) * 0.15f * lfo + S(100f, t) * 0.08f;
            s[i] += Ruido() * 0.02f;
            if (t > nextBeep && t < nextBeep + 0.05f)
            {
                float bt = t - nextBeep;
                float beepFreq = 1000f + (float)rng.NextDouble() * 500f;
                s[i] += S(beepFreq, bt) * 0.06f;
            }
            if (t > nextBeep + 0.05f)
                nextBeep += 4f + (float)rng.NextDouble() * 4f;
        }
        float[] buzz = new float[n];
        for (int i = 0; i < n; i++) buzz[i] = Ruido() * 0.02f;
        buzz = BandPass(buzz, 3000f, 5000f);
        for (int i = 0; i < n; i++) s[i] += buzz[i];
        Normalizar(s, 0.45f);
        return s;
    }

    // ===========================================================
    //  ESCRITOR WAV (PCM 16-bit mono 44100Hz)
    // ===========================================================

    static void Wav(string nombre, float[] samples)
    {
        string path = Application.dataPath + "/_Unity Essentials/Audio/Terror/" + nombre + ".wav";
        int numSamples = samples.Length;
        short channels = 1;
        short bps = 16;
        int byteRate = SR * channels * bps / 8;
        short blockAlign = (short)(channels * bps / 8);
        int dataSize = numSamples * blockAlign;

        using (var fs = new FileStream(path, FileMode.Create))
        using (var w = new BinaryWriter(fs))
        {
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            w.Write(36 + dataSize);
            w.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            w.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            w.Write(16);
            w.Write((short)1);
            w.Write(channels);
            w.Write(SR);
            w.Write(byteRate);
            w.Write(blockAlign);
            w.Write(bps);

            w.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            w.Write(dataSize);

            for (int i = 0; i < numSamples; i++)
            {
                short val = (short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767f);
                w.Write(val);
            }
        }
    }

    // ===========================================================
    //  ASIGNACION A MANAGERS
    // ===========================================================

    static void AsignarAudio()
    {
        AsignarGestor();
        AsignarTrampa();
        AsignarEscher();
        AsignarSentidos();
        AsignarAmbiente();
    }

    static void AsignarGestor()
    {
        var g = Object.FindAnyObjectByType<GestorHistoriaISTR>();
        if (g == null) return;
        var so = new SerializedObject(g);
        Clip(so, "sonidoAlarmaServidor", "alarma_servidor");
        Clip(so, "sonidoReinicioExitoso", "reinicio_exitoso");
        Clip(so, "sonidoPuertaDesbloqueo", "puerta_desbloqueo");
        Clip(so, "musicaGradasTension", "musica_gradas_tension");
        Clip(so, "sonidoAnomaliaFinal", "anomalia_final");
        Clip(so, "musicaClimaxFinal", "musica_climax_final");
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(g);
    }

    static void AsignarTrampa()
    {
        var t = Object.FindAnyObjectByType<TrampaGradasISTR>();
        if (t == null) return;
        var so = new SerializedObject(t);
        Clip(so, "sonidoFlickerLuz", "flicker_luz");
        Clip(so, "sonidoSusurroCercano", "susurro_cercano");
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(t);
    }

    static void AsignarEscher()
    {
        var e = Object.FindAnyObjectByType<EscaleraParadojaEscher>();
        if (e == null) return;
        var so = new SerializedObject(e);
        Clip(so, "sonidoDistorsionGrave", "distorsion_grave");
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(e);
    }

    static void AsignarSentidos()
    {
        var sv = Object.FindAnyObjectByType<ControladorSentidosVR>();
        if (sv == null) return;
        var so = new SerializedObject(sv);
        ClipArray(so, "clipsPasosFantasma",
            new[] { "paso_fantasma_1", "paso_fantasma_2", "paso_fantasma_3" });
        ClipArray(so, "clipsSusurrosGradas",
            new[] { "susurro_gradas_1", "susurro_gradas_2" });
        Clip(so, "latidoClip", "latido_cardiaco");
        ClipArray(so, "clipsTos", new[] { "tos_1", "tos_2" });
        Clip(so, "clipAlientoFrio", "aliento_frio");
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(sv);
    }

    static void AsignarAmbiente()
    {
        var a = Object.FindAnyObjectByType<AmbienciaAudioISTR>();
        if (a == null) return;
        var so = new SerializedObject(a);
        Clip(so, "ambienteMaquinas", "ambiente_maquinas");
        Clip(so, "ambienteGradas", "ambiente_gradas");
        Clip(so, "ambienteLaboratorio", "ambiente_laboratorio");
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(a);
    }

    static void Clip(SerializedObject so, string prop, string archivo)
    {
        var p = so.FindProperty(prop);
        if (p == null || p.objectReferenceValue != null) return;
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(RUTA + "/" + archivo + ".wav");
        if (clip != null) p.objectReferenceValue = clip;
    }

    static void ClipArray(SerializedObject so, string prop, string[] archivos)
    {
        var p = so.FindProperty(prop);
        if (p == null || p.arraySize > 0) return;
        p.arraySize = archivos.Length;
        for (int i = 0; i < archivos.Length; i++)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(RUTA + "/" + archivos[i] + ".wav");
            if (clip != null)
                p.GetArrayElementAtIndex(i).objectReferenceValue = clip;
        }
    }
}
#endif
