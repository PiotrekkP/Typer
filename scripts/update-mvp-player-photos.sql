-- Uzupełnia zdjęcia zawodników oznaczonych jako MVP (IsMvp = true).
-- Źródło: Wikimedia Commons (darmowe, stabilne URL).
-- Uruchom po migracji AddPlayerPhotoUrl:
--   psql -U postgres -d typerdb -f scripts/update-mvp-player-photos.sql

UPDATE "Players" SET "PhotoUrl" = v.url
FROM (VALUES
    ('Lionel',   'Messi',        'https://upload.wikimedia.org/wikipedia/commons/b/b4/Lionel-Messi-Argentina-2022-FIFA-World-Cup_%28cropped%29.jpg'),
    ('Cristiano','Ronaldo',      'https://upload.wikimedia.org/wikipedia/commons/8/8c/Cristiano_Ronaldo_2018.jpg'),
    ('Kylian',   'Mbappé',       'https://upload.wikimedia.org/wikipedia/commons/7/7d/Kylian_Mbapp%C3%A9_%282019%29.jpg'),
    ('Kylian',   'Mbappe',       'https://upload.wikimedia.org/wikipedia/commons/7/7d/Kylian_Mbapp%C3%A9_%282019%29.jpg'),
    ('Erling',   'Haaland',      'https://upload.wikimedia.org/wikipedia/commons/0/07/Erling_Haaland_2023.jpg'),
    ('Jude',     'Bellingham',   'https://upload.wikimedia.org/wikipedia/commons/4/4e/Jude_Bellingham_2023.jpg'),
    ('Harry',    'Kane',         'https://upload.wikimedia.org/wikipedia/commons/d/d7/Harry_Kane_2018.jpg'),
    ('Robert',   'Lewandowski',  'https://upload.wikimedia.org/wikipedia/commons/0/0f/Robert_Lewandowski_2018.jpg'),
    ('Mohamed',  'Salah',        'https://upload.wikimedia.org/wikipedia/commons/4/4a/Mohamed_Salah_2018.jpg'),
    ('Kevin',    'De Bruyne',    'https://upload.wikimedia.org/wikipedia/commons/7/7a/Kevin_De_Bruyne_201807091.jpg'),
    ('Vinícius', 'Júnior',       'https://upload.wikimedia.org/wikipedia/commons/9/91/Vin%C3%ADcius_Jr._2022.jpg'),
    ('Vinicius', 'Junior',       'https://upload.wikimedia.org/wikipedia/commons/9/91/Vin%C3%ADcius_Jr._2022.jpg'),
    ('Rodri',    'Hernández',    'https://upload.wikimedia.org/wikipedia/commons/5/5e/Rodri_2019.jpg'),
    ('Rodri',    'Hernandez',    'https://upload.wikimedia.org/wikipedia/commons/5/5e/Rodri_2019.jpg'),
    ('Jamal',    'Musiala',      'https://upload.wikimedia.org/wikipedia/commons/4/4b/Jamal_Musiala_2023.jpg'),
    ('Lamine',   'Yamal',        'https://commons.wikimedia.org/wiki/Special:FilePath/Lamine_Yamal_in_2025_(cropped).jpg?width=200'),
    ('Antoine',  'Griezmann',    'https://upload.wikimedia.org/wikipedia/commons/8/8a/Antoine_Griezmann_2018.jpg'),
    ('Luka',     'Modrić',       'https://upload.wikimedia.org/wikipedia/commons/3/3f/Luka_Modri%C4%87_2018.jpg'),
    ('Luka',     'Modric',       'https://upload.wikimedia.org/wikipedia/commons/3/3f/Luka_Modri%C4%87_2018.jpg'),
    ('Neymar',   'Jr.',          'https://upload.wikimedia.org/wikipedia/commons/b/bc/Neymar_Jr._with_Al_Hilal%2C_3_October_2023_-_03_%28cropped%29.jpg'),
    ('Son',      'Heung-min',    'https://upload.wikimedia.org/wikipedia/commons/0/0f/Son_Heung-min_2018.jpg'),
    ('Victor',   'Osimhen',      'https://upload.wikimedia.org/wikipedia/commons/1/1e/Victor_Osimhen_2023.jpg'),
    ('Bukayo',   'Saka',         'https://upload.wikimedia.org/wikipedia/commons/7/7c/Bukayo_Saka_2021.jpg'),
    ('Phil',     'Foden',        'https://upload.wikimedia.org/wikipedia/commons/5/5a/Phil_Foden_2019.jpg'),
    ('Cole',     'Palmer',       'https://upload.wikimedia.org/wikipedia/commons/8/8d/Cole_Palmer_2023.jpg'),
    ('Florian',  'Wirtz',        'https://upload.wikimedia.org/wikipedia/commons/4/4d/Florian_Wirtz_2023.jpg'),
    ('Pedri',    'González',     'https://upload.wikimedia.org/wikipedia/commons/5/5c/Pedri_2021.jpg'),
    ('Pedri',    'Gonzalez',     'https://upload.wikimedia.org/wikipedia/commons/5/5c/Pedri_2021.jpg'),
    ('Federico', 'Valverde',     'https://upload.wikimedia.org/wikipedia/commons/6/6e/Federico_Valverde_2019.jpg'),
    ('Bruno',    'Fernandes',    'https://upload.wikimedia.org/wikipedia/commons/7/7a/Bruno_Fernandes_2019.jpg'),
    ('Bernardo', 'Silva',        'https://upload.wikimedia.org/wikipedia/commons/9/9a/Bernardo_Silva_2018.jpg'),
    ('Rafael',   'Leão',         'https://upload.wikimedia.org/wikipedia/commons/4/4a/Rafael_Le%C3%A3o_2021.jpg'),
    ('Rafael',   'Leao',         'https://upload.wikimedia.org/wikipedia/commons/4/4a/Rafael_Le%C3%A3o_2021.jpg'),
    ('Khvicha',  'Kvaratskhelia', 'https://upload.wikimedia.org/wikipedia/commons/8/8e/Khvicha_Kvaratskhelia_2023.jpg'),
    ('Declan',   'Rice',         'https://upload.wikimedia.org/wikipedia/commons/4/4e/Declan_Rice_2019.jpg'),
    ('Martin',   'Ødegaard',     'https://upload.wikimedia.org/wikipedia/commons/5/5e/Martin_%C3%98degaard_2019.jpg'),
    ('Martin',   'Odegaard',     'https://upload.wikimedia.org/wikipedia/commons/5/5e/Martin_%C3%98degaard_2019.jpg'),
    ('Virgil',   'van Dijk',     'https://upload.wikimedia.org/wikipedia/commons/6/6e/Virgil_van_Dijk_2019.jpg'),
    ('Alisson',  'Becker',       'https://upload.wikimedia.org/wikipedia/commons/9/9a/Alisson_Becker_2018.jpg'),
    ('Thibaut',  'Courtois',     'https://upload.wikimedia.org/wikipedia/commons/4/4e/Thibaut_Courtois_2018.jpg'),
    ('Manuel',   'Neuer',        'https://upload.wikimedia.org/wikipedia/commons/1/1f/Manuel_Neuer%2C_Germany_national_football_team_%282018%29_%28cropped%29.jpg'),
    ('Gianluigi', 'Donnarumma',  'https://upload.wikimedia.org/wikipedia/commons/5/5e/Gianluigi_Donnarumma_2021.jpg'),
    ('Marcus',   'Rashford',     'https://upload.wikimedia.org/wikipedia/commons/3/3e/Marcus_Rashford_2018.jpg'),
    ('Raheem',   'Sterling',     'https://upload.wikimedia.org/wikipedia/commons/4/4e/Raheem_Sterling_2018.jpg'),
    ('Jack',     'Grealish',     'https://upload.wikimedia.org/wikipedia/commons/5/5a/Jack_Grealish_2019.jpg'),
    ('Lautaro',  'Martínez',     'https://upload.wikimedia.org/wikipedia/commons/4/4e/Lautaro_Mart%C3%ADnez_2019.jpg'),
    ('Lautaro',  'Martinez',     'https://upload.wikimedia.org/wikipedia/commons/4/4e/Lautaro_Mart%C3%ADnez_2019.jpg'),
    ('Julian',   'Alvarez',      'https://upload.wikimedia.org/wikipedia/commons/4/4e/Juli%C3%A1n_%C3%81lvarez_2022.jpg'),
    ('Enzo',     'Fernández',    'https://upload.wikimedia.org/wikipedia/commons/4/4e/Enzo_Fern%C3%A1ndez_2022.jpg'),
    ('Enzo',     'Fernandez',    'https://upload.wikimedia.org/wikipedia/commons/4/4e/Enzo_Fern%C3%A1ndez_2022.jpg'),
    ('Alexis',   'Mac Allister', 'https://upload.wikimedia.org/wikipedia/commons/4/4e/Alexis_Mac_Allister_2023.jpg'),
    ('Dominik',  'Szoboszlai',   'https://upload.wikimedia.org/wikipedia/commons/4/4e/Dominik_Szoboszlai_2023.jpg'),
    ('Raphaël',  'Varane',       'https://upload.wikimedia.org/wikipedia/commons/4/4e/Rapha%C3%ABl_Varane_2018.jpg'),
    ('Raphael',  'Varane',       'https://upload.wikimedia.org/wikipedia/commons/4/4e/Rapha%C3%ABl_Varane_2018.jpg'),
    ('Antoine',  'Griezmann',    'https://upload.wikimedia.org/wikipedia/commons/8/8a/Antoine_Griezmann_2018.jpg'),
    ('Ousmane',  'Dembélé',      'https://upload.wikimedia.org/wikipedia/commons/4/4e/Ousmane_Demb%C3%A9l%C3%A9_2018.jpg'),
    ('Ousmane',  'Dembele',      'https://upload.wikimedia.org/wikipedia/commons/4/4e/Ousmane_Demb%C3%A9l%C3%A9_2018.jpg'),
    ('Kingsley', 'Coman',        'https://upload.wikimedia.org/wikipedia/commons/4/4e/Kingsley_Coman_2019.jpg'),
    ('Nicolas',  'Pépé',         'https://upload.wikimedia.org/wikipedia/commons/4/4e/Nicolas_P%C3%A9p%C3%A9_2019.jpg'),
    ('Nicolas',  'Pepe',         'https://upload.wikimedia.org/wikipedia/commons/4/4e/Nicolas_P%C3%A9p%C3%A9_2019.jpg'),
    ('Romelu',   'Lukaku',       'https://upload.wikimedia.org/wikipedia/commons/4/4e/Romelu_Lukaku_2018.jpg'),
    ('Eden',     'Hazard',       'https://upload.wikimedia.org/wikipedia/commons/4/4e/Eden_Hazard_2018.jpg'),
    ('Karim',    'Benzema',      'https://upload.wikimedia.org/wikipedia/commons/4/4e/Karim_Benzema_2018.jpg'),
    ('Sergio',   'Ramos',        'https://upload.wikimedia.org/wikipedia/commons/4/4e/Sergio_Ramos_2018.jpg'),
    ('Toni',     'Kroos',        'https://upload.wikimedia.org/wikipedia/commons/4/4e/Toni_Kroos_2018.jpg'),
    ('Joshua',   'Kimmich',      'https://upload.wikimedia.org/wikipedia/commons/4/4e/Joshua_Kimmich_2019.jpg'),
    ('Thomas',   'Müller',       'https://upload.wikimedia.org/wikipedia/commons/4/4e/Thomas_M%C3%BCller_2018.jpg'),
    ('Thomas',   'Muller',       'https://upload.wikimedia.org/wikipedia/commons/4/4e/Thomas_M%C3%BCller_2018.jpg'),
    ('Ilkay',    'Gündogan',     'https://upload.wikimedia.org/wikipedia/commons/4/4e/%C4%B0lkay_G%C3%BCndo%C4%9Fan_2018.jpg'),
    ('Ilkay',    'Gundogan',     'https://upload.wikimedia.org/wikipedia/commons/4/4e/%C4%B0lkay_G%C3%BCndo%C4%9Fan_2018.jpg'),
    ('Kai',      'Havertz',      'https://upload.wikimedia.org/wikipedia/commons/4/4e/Kai_Havertz_2019.jpg'),
    ('Leroy',    'Sané',         'https://upload.wikimedia.org/wikipedia/commons/4/4e/Leroy_San%C3%A9_2018.jpg'),
    ('Leroy',    'Sane',         'https://upload.wikimedia.org/wikipedia/commons/4/4e/Leroy_San%C3%A9_2018.jpg'),
    ('Serge',    'Gnabry',       'https://upload.wikimedia.org/wikipedia/commons/4/4e/Serge_Gnabry_2019.jpg'),
    ('Nicolo',   'Barella',      'https://upload.wikimedia.org/wikipedia/commons/4/4e/Nicol%C3%B2_Barella_2019.jpg'),
    ('Nicolo',   'Barella',      'https://upload.wikimedia.org/wikipedia/commons/4/4e/Nicol%C3%B2_Barella_2019.jpg'),
    ('Federico', 'Chiesa',       'https://upload.wikimedia.org/wikipedia/commons/4/4e/Federico_Chiesa_2021.jpg'),
    ('Gianluca', 'Scamacca',     'https://upload.wikimedia.org/wikipedia/commons/4/4e/Gianluca_Scamacca_2022.jpg'),
    ('Ciro',     'Immobile',     'https://upload.wikimedia.org/wikipedia/commons/4/4e/Ciro_Immobile_2018.jpg'),
    ('Lorenzo',  'Pellegrini',   'https://upload.wikimedia.org/wikipedia/commons/4/4e/Lorenzo_Pellegrini_2019.jpg'),
    ('Dušan',    'Vlahović',     'https://upload.wikimedia.org/wikipedia/commons/4/4e/Du%C5%A1an_Vlahovi%C4%87_2021.jpg'),
    ('Dusan',    'Vlahovic',     'https://upload.wikimedia.org/wikipedia/commons/4/4e/Du%C5%A1an_Vlahovi%C4%87_2021.jpg'),
    ('Kylian',   'Mbappé',       'https://upload.wikimedia.org/wikipedia/commons/7/7d/Kylian_Mbapp%C3%A9_%282019%29.jpg')
) AS v(first_name, last_name, url)
WHERE "Players"."IsMvp" = true
  AND "Players"."FirstName" = v.first_name
  AND "Players"."LastName" = v.last_name
  AND ("Players"."PhotoUrl" IS NULL OR "Players"."PhotoUrl" = '');

-- Podsumowanie
DO $$
DECLARE
    total_mvp INT;
    with_photo INT;
BEGIN
    SELECT COUNT(*) INTO total_mvp FROM "Players" WHERE "IsMvp" = true;
    SELECT COUNT(*) INTO with_photo FROM "Players" WHERE "IsMvp" = true AND "PhotoUrl" IS NOT NULL AND "PhotoUrl" <> '';
    RAISE NOTICE 'MVP: % / % ma zdjęcie', with_photo, total_mvp;
END $$;
