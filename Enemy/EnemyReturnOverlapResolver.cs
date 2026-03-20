using System;
using System.Collections.Generic;

// 복귀한 적의 X 위치를 보정해 편대 내 겹침을 방지한다.
public static class EnemyReturnOverlapResolver
{
    private const int k_EnemyMinX = 4;
    private const int k_EnemyMaxX = 35;

    // current: 복귀 완료된 적, enemies: 현재 화면의 적 목록
    public static void Apply(Enemy current, List<Enemy> enemies)
    {
        // originalX: 복귀 직후의 기준 X(우선 유지 시도)
        int originalX = Math.Clamp(current.X, k_EnemyMinX, k_EnemyMaxX);

        // 현재 위치가 이미 안전하면 그대로 확정
        if (!HasSpacingConflictAtX(current, enemies, originalX))
        {
            current.ChargeTargetX = current.X;
            return;
        }

        // minX/maxX: 복귀 후 유지해야 하는 편대 X 범위(고정 4..35)
        int minX = k_EnemyMinX;
        int maxX = k_EnemyMaxX;

        // 좌/우 여유 칸 수를 비교해 더 여유 있는 쪽을 먼저 탐색
        int leftRoom = originalX - minX;
        int rightRoom = maxX - originalX;
        bool tryLeftFirst = rightRoom < leftRoom;

        // 원점에서 최대 이동 가능한 거리만큼 양방향 후보를 탐색
        int maxStep = Math.Max(maxX - originalX, originalX - minX);
        for (int step = 1; step <= maxStep; step++)
        {
            // 우선 탐색 방향 후보
            int firstCandidateX = tryLeftFirst ? originalX - step : originalX + step;
            if (firstCandidateX >= minX && firstCandidateX <= maxX &&
                !HasSpacingConflictAtX(current, enemies, firstCandidateX))
            {
                current.X = firstCandidateX;
                current.ChargeTargetX = current.X;
                return;
            }

            // 반대 방향 후보
            int secondCandidateX = tryLeftFirst ? originalX + step : originalX - step;
            if (secondCandidateX >= minX && secondCandidateX <= maxX &&
                !HasSpacingConflictAtX(current, enemies, secondCandidateX))
            {
                current.X = secondCandidateX;
                current.ChargeTargetX = current.X;
                return;
            }
        }

        current.X = originalX;
        current.ChargeTargetX = current.X;
    }

    private static bool HasSpacingConflictAtX(Enemy current, List<Enemy> enemies, int candidateX)
    {
        // candidateX에 current를 둔다고 가정한 가상 점유 범위
        int currentHalf = GetHalfWidth(current);
        int currentLeft = candidateX - currentHalf;
        int currentRight = candidateX + currentHalf;

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy other = enemies[i];
            if (other == null || !other.IsActive || other.IsShowingEffect || ReferenceEquals(other, current))
            {
                continue;
            }

            if (other.Y != current.Y)
            {
                continue;
            }

            // other의 실제 점유 범위
            int otherHalf = GetHalfWidth(other);
            int otherLeft = other.X - otherHalf;
            int otherRight = other.X + otherHalf;
            // 보스 포함 여부에 따라 필요한 최소 간격
            int requiredGap = GetRequiredGap(current, other);

            // 두 구간이 requiredGap을 고려해 겹치거나 너무 가까우면 충돌
            if (currentRight + requiredGap >= otherLeft && currentLeft - requiredGap <= otherRight)
            {
                return true;
            }
        }

        return false;
    }

    private static int GetRequiredGap(Enemy a, Enemy b)
    {
        bool hasBoss = GetHalfWidth(a) == 2 || GetHalfWidth(b) == 2;
        return hasBoss ? 0 : 1;
    }

    private static int GetHalfWidth(Enemy enemy)
    {
        return (enemy.Type == Enemy.EnemyType.Boss1 || enemy.Type == Enemy.EnemyType.Boss2 ||
                enemy.Type == Enemy.EnemyType.Boss1_Rush || enemy.Type == Enemy.EnemyType.Boss2_Rush) ? 2 : 1;
    }
}
