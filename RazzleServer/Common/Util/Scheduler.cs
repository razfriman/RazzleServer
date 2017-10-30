using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Util
{
    public static class Scheduler
    {
        /// <summary>
        /// Schedules an action to be invoked after a delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ScheduleDelayedAction(Action action, int delay, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            await Task.Delay(delay, cancellationToken);
            action();
        }

        /// <summary>
        /// Schedules an action to be invoked after a delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ScheduleRepeatingAction(Action action, int delay, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            await Task.Delay(delay, cancellationToken);
            action();
            await ScheduleDelayedAction(action, delay);
        }

        /// <summary>
        /// Cooltime in milliseconds
        /// </summary>
        /// <param name="source"></param>
        /// <param name="skillId"></param>
        /// <param name="delay"></param>
        public static Task ScheduleRemoveCooldown(Character source, int skillId, int delay, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ScheduleDelayedAction(new Action(() =>
            {
                if (source != null && source.Client != null)
                    source.RemoveCooldown(skillId);
            }), delay, cancellationToken);
        }

        /// <summary>
        /// Cooltime in milliseconds
        /// </summary>
        /// <param name="source"></param>
        /// <param name="skillIds"></param>
        /// <param name="delay"></param>
        public static Task ScheduleRemoveCooldowns(Character source, List<int> skillIds, int delay, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ScheduleDelayedAction(new Action(() =>
            {
                if (source != null && source.Client != null)
                {
                    foreach (int i in skillIds)
                        source.RemoveCooldown(i);
                }
            }), delay, cancellationToken);
        }

        /// <summary>
        /// Duration in milliseconds
        /// </summary>
        /// <param name="source"></param>
        /// <param name="skillId"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static Task ScheduleRemoveBuff(Character source, int skillId, int delay, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ScheduleDelayedAction(new Action(() =>
            {
                if (source != null && source.Client != null)
                    source.Buffs.Remove(skillId);
            }), delay, cancellationToken);
        }

        public static Task ScheduleRemoveSummon(Character source, int summonSkillId, int delay, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ScheduleDelayedAction(new Action(() =>
            {
                if (source != null && source.Client != null)
                    source.Summons(summonSkillId);
            }), delay, cancellationToken);
        }

		public static Task ScheduleRemoveMonsterStatusEffect(MonsterBuff effect, int delay, CancellationToken cancellationToken = default(CancellationToken)) => ScheduleDelayedAction(new Action(() =>
		{
			effect.Dispose(false);
		}), delay, cancellationToken);
    }
}