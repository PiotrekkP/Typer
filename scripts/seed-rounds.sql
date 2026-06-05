DO $$
DECLARE
    v_season_id UUID;
    v_r1 UUID;
    v_r2 UUID;
    v_r3 UUID;
    v_r4 UUID;
    v_r5 UUID;
BEGIN
    SELECT "Id" INTO v_season_id FROM "Seasons" ORDER BY "CreatedAt" LIMIT 1;

    v_r1 := gen_random_uuid();
    v_r2 := gen_random_uuid();
    v_r3 := gen_random_uuid();
    v_r4 := gen_random_uuid();
    v_r5 := gen_random_uuid();

    INSERT INTO "Rounds" ("Id", "SeasonId", "Name", "OrderNumber", "CreatedAt")
    VALUES
        (v_r1, v_season_id, 'Faza grupowa - Kolejka 1', 1, NOW()),
        (v_r2, v_season_id, 'Faza grupowa - Kolejka 2', 2, NOW()),
        (v_r3, v_season_id, 'Faza grupowa - Kolejka 3', 3, NOW()),
        (v_r4, v_season_id, 'Runda 32',                 4, NOW()),
        (v_r5, v_season_id, '1/8 finału',               5, NOW());

    -- Przypisz mecze do kolejek według kolejności chronologicznej
    -- 16 grup × 3 mecze = 48 grupowych, potem 16 + 8 = 24 pucharowe
    UPDATE "Matches"
    SET "RoundId" =
        CASE
            WHEN rn BETWEEN  1 AND 16 THEN v_r1
            WHEN rn BETWEEN 17 AND 32 THEN v_r2
            WHEN rn BETWEEN 33 AND 48 THEN v_r3
            WHEN rn BETWEEN 49 AND 64 THEN v_r4
            ELSE                            v_r5
        END
    FROM (
        SELECT "Id" AS mid,
               ROW_NUMBER() OVER (ORDER BY "KickOffUtc") AS rn
        FROM "Matches"
    ) ranked
    WHERE "Matches"."Id" = ranked.mid;

    RAISE NOTICE 'Dodano 5 kolejek i przypisano % meczów.',
        (SELECT COUNT(*) FROM "Matches" WHERE "RoundId" IS NOT NULL);
END $$;
