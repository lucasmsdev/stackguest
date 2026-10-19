using System;
using System.Linq;
using System.Windows.Forms;

namespace DockerQuest
{
    // Sons do escritório: passos (conforme o piso), vozes nas conversas, murmúrio e teclados ao fundo.
    partial class GameView
    {
        int lastStep;
        float ambienceTimer;

        // tipo de piso no ponto: 0 piso frio, 1 madeira, 2 carpete, 3 piso elevado, 4 grama (corredores = piso frio)
        int FloorAt(float x, float y)
        {
            int z = World.ZoneAt(x, y);
            if (z < 0) return 0;
            int st = Story.SectorOf(z).Style;
            return st >= 0 && st <= 4 ? st : 0;
        }

        // um passo a cada meia volta do ciclo de caminhada (quando o pé toca o chão)
        void PlayerSteps(bool running)
        {
            int s = (int)Math.Floor(walk / Math.PI);
            if (s != lastStep && moving) Audio.Step(FloorAt(px, py), running ? 0.5f : 0.36f);
            lastStep = s;
        }

        void NpcStep(Npc n)
        {
            int s = (int)Math.Floor(n.Walk / Math.PI);
            if (s == n.LastStep) return;
            n.LastStep = s;
            float d = (float)Math.Sqrt((n.X - px) * (n.X - px) + (n.Y - py) * (n.Y - py));
            if (d > 9f) return;
            float k = 1f - d / 9f;
            Audio.Step(FloorAt(n.X, n.Y), 0.24f * k * k);
        }

        // tom de voz de cada pessoa (fixo, a partir do nome)
        static float NpcPitch(Npc n)
        {
            if (n == null) return 1f;
            int h = 17;
            foreach (char c in n.Name) h = h * 31 + c;
            return 0.78f + (Math.Abs(h) % 60) / 100f;
        }

        void NpcChatter(Npc n, int syllables)
        {
            if (n == null) return;
            float d = (float)Math.Sqrt((n.X - px) * (n.X - px) + (n.Y - py) * (n.Y - py));
            if (d > 12f) return;
            float k = 1f - d / 12f;
            Audio.Chatter(NpcPitch(n), 0.22f * k * k + 0.03f, syllables);
        }

        // ambiente: mais gente perto = mais conversa; gente trabalhando nas mesas = teclados
        void UpdateAmbience(float dt)
        {
            ambienceTimer -= dt;
            if (ambienceTimer > 0) return;
            ambienceTimer = 0.25f;
            int zone = World.ZoneAt(px, py);
            int people = 0, workers = 0;
            foreach (var n in World.Npcs)
            {
                float d = Math.Abs(n.X - px) + Math.Abs(n.Y - py);
                if (d > 11f) continue;
                people++;
                int nz = World.ZoneAt(n.X, n.Y);
                bool desk = nz >= 0 && (Story.SectorOf(nz).Cat != null || nz == Story.Soc || nz == Story.HelpDesk || nz == Story.Innovation || nz == Story.Library);
                if (desk && !n.Wander && !n.Stroll && n.Kind != NpcKind.Mentor) workers++;
            }
            float place = zone == Story.Cafeteria ? 1.7f : zone == Story.GameRoom ? 1.3f : zone == Story.Auditorium ? 0.7f
                        : zone == Story.Library ? 0.25f : zone == Story.Garden ? 0.45f : zone < 0 ? 0.9f : 1f;
            float murmur = Math.Min(0.3f, 0.045f * people) * place;
            float typing = zone == Story.Cafeteria || zone == Story.Garden || zone == Story.GameRoom || zone == Story.Auditorium ? 0.05f : workers * 0.55f + (zone >= 0 ? 0.15f : 0.05f);
            bool active = Form.ActiveForm != null;
            float master = !active ? 0f : Paused ? 0.35f : page != null ? 0.7f : 1f;
            Audio.SetAmbience(murmur, typing, 0.07f, master);
        }
    }
}
