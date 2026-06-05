using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Typer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedMvpPlayerPhotoUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Commons Special:FilePath — działa w przeglądarce z referrerpolicy="no-referrer"
            migrationBuilder.Sql("""
                UPDATE "Players" SET "PhotoUrl" = v.url
                FROM (VALUES
                    ('Lionel',    'Messi',         'https://commons.wikimedia.org/wiki/Special:FilePath/Lionel_Messi_20180626.jpg?width=128'),
                    ('Cristiano', 'Ronaldo',       'https://commons.wikimedia.org/wiki/Special:FilePath/Cristiano_Ronaldo_2018.jpg?width=128'),
                    ('Kylian',    'Mbappé',        'https://commons.wikimedia.org/wiki/Special:FilePath/Kylian_Mbapp%C3%A9_2018.jpg?width=128'),
                    ('Kylian',    'Mbappe',        'https://commons.wikimedia.org/wiki/Special:FilePath/Kylian_Mbapp%C3%A9_2018.jpg?width=128'),
                    ('Erling',    'Haaland',       'https://commons.wikimedia.org/wiki/Special:FilePath/Erling_Haaland_2023.jpg?width=128'),
                    ('Jude',      'Bellingham',    'https://commons.wikimedia.org/wiki/Special:FilePath/Jude_Bellingham_2023.jpg?width=128'),
                    ('Harry',     'Kane',          'https://commons.wikimedia.org/wiki/Special:FilePath/Harry_Kane_2018.jpg?width=128'),
                    ('Robert',    'Lewandowski',   'https://commons.wikimedia.org/wiki/Special:FilePath/Robert_Lewandowski_2018.jpg?width=128'),
                    ('Mohamed',   'Salah',         'https://commons.wikimedia.org/wiki/Special:FilePath/Mohamed_Salah_2018.jpg?width=128'),
                    ('Kevin',     'De Bruyne',     'https://commons.wikimedia.org/wiki/Special:FilePath/Kevin_De_Bruyne_201807091.jpg?width=128'),
                    ('Vinícius',  'Júnior',        'https://commons.wikimedia.org/wiki/Special:FilePath/Vin%C3%ADcius_Jr._2022.jpg?width=128'),
                    ('Vinicius',  'Junior',        'https://commons.wikimedia.org/wiki/Special:FilePath/Vin%C3%ADcius_Jr._2022.jpg?width=128'),
                    ('Jamal',     'Musiala',       'https://commons.wikimedia.org/wiki/Special:FilePath/Jamal_Musiala_2023.jpg?width=128'),
                    ('Lamine',    'Yamal',         'https://commons.wikimedia.org/wiki/Special:FilePath/Lamine_Yamal_in_2025_(cropped).jpg?width=200'),
                    ('Antoine',   'Griezmann',     'https://commons.wikimedia.org/wiki/Special:FilePath/Antoine_Griezmann_2018.jpg?width=128'),
                    ('Luka',      'Modrić',        'https://commons.wikimedia.org/wiki/Special:FilePath/Luka_Modri%C4%87_2018.jpg?width=128'),
                    ('Luka',      'Modric',        'https://commons.wikimedia.org/wiki/Special:FilePath/Luka_Modri%C4%87_2018.jpg?width=128'),
                    ('Bukayo',    'Saka',          'https://commons.wikimedia.org/wiki/Special:FilePath/Bukayo_Saka_2021.jpg?width=128'),
                    ('Phil',      'Foden',         'https://commons.wikimedia.org/wiki/Special:FilePath/Phil_Foden_2019.jpg?width=128'),
                    ('Cole',      'Palmer',        'https://commons.wikimedia.org/wiki/Special:FilePath/Cole_Palmer_2023.jpg?width=128'),
                    ('Florian',   'Wirtz',         'https://commons.wikimedia.org/wiki/Special:FilePath/Florian_Wirtz_2023.jpg?width=128'),
                    ('Pedri',     'González',      'https://commons.wikimedia.org/wiki/Special:FilePath/Pedri_2021.jpg?width=128'),
                    ('Pedri',     'Gonzalez',      'https://commons.wikimedia.org/wiki/Special:FilePath/Pedri_2021.jpg?width=128'),
                    ('Rodri',     'Hernández',     'https://commons.wikimedia.org/wiki/Special:FilePath/Rodri_2019.jpg?width=128'),
                    ('Rodri',     'Hernandez',     'https://commons.wikimedia.org/wiki/Special:FilePath/Rodri_2019.jpg?width=128'),
                    ('Federico',  'Valverde',      'https://commons.wikimedia.org/wiki/Special:FilePath/Federico_Valverde_2019.jpg?width=128'),
                    ('Bruno',     'Fernandes',     'https://commons.wikimedia.org/wiki/Special:FilePath/Bruno_Fernandes_2019.jpg?width=128'),
                    ('Declan',    'Rice',          'https://commons.wikimedia.org/wiki/Special:FilePath/Declan_Rice_2019.jpg?width=128'),
                    ('Martin',    'Ødegaard',      'https://commons.wikimedia.org/wiki/Special:FilePath/Martin_%C3%98degaard_2019.jpg?width=128'),
                    ('Martin',    'Odegaard',      'https://commons.wikimedia.org/wiki/Special:FilePath/Martin_%C3%98degaard_2019.jpg?width=128'),
                    ('Virgil',    'van Dijk',      'https://commons.wikimedia.org/wiki/Special:FilePath/Virgil_van_Dijk_2019.jpg?width=128'),
                    ('Alisson',   'Becker',        'https://commons.wikimedia.org/wiki/Special:FilePath/Alisson_Becker_2018.jpg?width=128'),
                    ('Thibaut',   'Courtois',      'https://commons.wikimedia.org/wiki/Special:FilePath/Thibaut_Courtois_2018.jpg?width=128'),
                    ('Manuel',    'Neuer',         'https://commons.wikimedia.org/wiki/Special:FilePath/Manuel_Neuer%2C_Germany_national_football_team_%282018%29_%28cropped%29.jpg?width=128'),
                    ('Joshua',    'Kimmich',       'https://commons.wikimedia.org/wiki/Special:FilePath/Joshua_Kimmich_2019.jpg?width=128'),
                    ('Thomas',    'Müller',        'https://commons.wikimedia.org/wiki/Special:FilePath/Thomas_M%C3%BCller_2018.jpg?width=128'),
                    ('Thomas',    'Muller',        'https://commons.wikimedia.org/wiki/Special:FilePath/Thomas_M%C3%BCller_2018.jpg?width=128'),
                    ('Kai',       'Havertz',       'https://commons.wikimedia.org/wiki/Special:FilePath/Kai_Havertz_2019.jpg?width=128'),
                    ('Lautaro',   'Martínez',      'https://commons.wikimedia.org/wiki/Special:FilePath/Lautaro_Mart%C3%ADnez_2019.jpg?width=128'),
                    ('Lautaro',   'Martinez',      'https://commons.wikimedia.org/wiki/Special:FilePath/Lautaro_Mart%C3%ADnez_2019.jpg?width=128'),
                    ('Julian',    'Alvarez',       'https://commons.wikimedia.org/wiki/Special:FilePath/Juli%C3%A1n_%C3%81lvarez_2022.jpg?width=128'),
                    ('Enzo',      'Fernández',     'https://commons.wikimedia.org/wiki/Special:FilePath/Enzo_Fern%C3%A1ndez_2022.jpg?width=128'),
                    ('Enzo',      'Fernandez',     'https://commons.wikimedia.org/wiki/Special:FilePath/Enzo_Fern%C3%A1ndez_2022.jpg?width=128'),
                    ('Alexis',    'Mac Allister',  'https://commons.wikimedia.org/wiki/Special:FilePath/Alexis_Mac_Allister_2023.jpg?width=128'),
                    ('Dominik',   'Szoboszlai',    'https://commons.wikimedia.org/wiki/Special:FilePath/Dominik_Szoboszlai_2023.jpg?width=128'),
                    ('Federico',  'Chiesa',        'https://commons.wikimedia.org/wiki/Special:FilePath/Federico_Chiesa_2021.jpg?width=128'),
                    ('Victor',    'Osimhen',       'https://commons.wikimedia.org/wiki/Special:FilePath/Victor_Osimhen_2023.jpg?width=128'),
                    ('Son',       'Heung-min',     'https://commons.wikimedia.org/wiki/Special:FilePath/Son_Heung-min_2018.jpg?width=128'),
                    ('Neymar',    'Jr.',           'https://commons.wikimedia.org/wiki/Special:FilePath/Neymar_Jr._with_Al_Hilal%2C_3_October_2023_-_03_%28cropped%29.jpg?width=128'),
                    ('Marcus',    'Rashford',      'https://commons.wikimedia.org/wiki/Special:FilePath/Marcus_Rashford_2018.jpg?width=128'),
                    ('Jack',      'Grealish',      'https://commons.wikimedia.org/wiki/Special:FilePath/Jack_Grealish_2019.jpg?width=128'),
                    ('Ousmane',   'Dembélé',       'https://commons.wikimedia.org/wiki/Special:FilePath/Ousmane_Demb%C3%A9l%C3%A9_2018.jpg?width=128'),
                    ('Ousmane',   'Dembele',       'https://commons.wikimedia.org/wiki/Special:FilePath/Ousmane_Demb%C3%A9l%C3%A9_2018.jpg?width=128'),
                    ('Kingsley',  'Coman',         'https://commons.wikimedia.org/wiki/Special:FilePath/Kingsley_Coman_2019.jpg?width=128'),
                    ('Romelu',    'Lukaku',        'https://commons.wikimedia.org/wiki/Special:FilePath/Romelu_Lukaku_2018.jpg?width=128'),
                    ('Karim',     'Benzema',       'https://commons.wikimedia.org/wiki/Special:FilePath/Karim_Benzema_2018.jpg?width=128'),
                    ('Sergio',    'Ramos',         'https://commons.wikimedia.org/wiki/Special:FilePath/Sergio_Ramos_2018.jpg?width=128'),
                    ('Toni',      'Kroos',         'https://commons.wikimedia.org/wiki/Special:FilePath/Toni_Kroos_2018.jpg?width=128'),
                    ('Ilkay',     'Gündogan',      'https://commons.wikimedia.org/wiki/Special:FilePath/%C4%B0lkay_G%C3%BCndo%C4%9Fan_2018.jpg?width=128'),
                    ('Ilkay',     'Gundogan',      'https://commons.wikimedia.org/wiki/Special:FilePath/%C4%B0lkay_G%C3%BCndo%C4%9Fan_2018.jpg?width=128'),
                    ('Nicolo',    'Barella',       'https://commons.wikimedia.org/wiki/Special:FilePath/Nicol%C3%B2_Barella_2019.jpg?width=128'),
                    ('Raphaël',   'Varane',        'https://commons.wikimedia.org/wiki/Special:FilePath/Rapha%C3%ABl_Varane_2018.jpg?width=128'),
                    ('Raphael',   'Varane',        'https://commons.wikimedia.org/wiki/Special:FilePath/Rapha%C3%ABl_Varane_2018.jpg?width=128'),
                    ('Gianluigi', 'Donnarumma',    'https://commons.wikimedia.org/wiki/Special:FilePath/Gianluigi_Donnarumma_2021.jpg?width=128'),
                    ('Khvicha',   'Kvaratskhelia', 'https://commons.wikimedia.org/wiki/Special:FilePath/Khvicha_Kvaratskhelia_2023.jpg?width=128'),
                    ('Bernardo',  'Silva',         'https://commons.wikimedia.org/wiki/Special:FilePath/Bernardo_Silva_2018.jpg?width=128'),
                    ('Rafael',    'Leão',          'https://commons.wikimedia.org/wiki/Special:FilePath/Rafael_Le%C3%A3o_2021.jpg?width=128'),
                    ('Rafael',    'Leao',          'https://commons.wikimedia.org/wiki/Special:FilePath/Rafael_Le%C3%A3o_2021.jpg?width=128')
                ) AS v(first_name, last_name, url)
                WHERE "Players"."IsMvp" = true
                  AND "Players"."FirstName" = v.first_name
                  AND "Players"."LastName" = v.last_name
                  AND (
                      "Players"."PhotoUrl" IS NULL
                      OR "Players"."PhotoUrl" = ''
                      OR "Players"."PhotoUrl" LIKE 'https://upload.wikimedia.org/%'
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Dane referencyjne — bez rollbacku
        }
    }
}
