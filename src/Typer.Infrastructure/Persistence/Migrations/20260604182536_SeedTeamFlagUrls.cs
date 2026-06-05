using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Typer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedTeamFlagUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/mx.svg' WHERE ""Id"" = '77d2f39e-1f7e-4f0e-bd59-be4a6846ad35';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/za.svg' WHERE ""Id"" = 'aac55c41-7b82-47d8-a3ea-047fdd8c213a';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/kr.svg' WHERE ""Id"" = '5213f61e-b6ea-49e2-8213-106e07912c50';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/cz.svg' WHERE ""Id"" = 'e1015d0c-8670-4f5b-9710-09a9d1f4bbb7';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/ca.svg' WHERE ""Id"" = '9ffa9abc-e05f-4105-99d9-dbc8029dcfc9';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/ba.svg' WHERE ""Id"" = '6f361c55-e283-451c-b21c-85b70aff46fc';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/qa.svg' WHERE ""Id"" = '18bbe3a1-3a5e-44ed-af0a-426d9778fdb5';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/ch.svg' WHERE ""Id"" = '2547ec98-3cbf-44e2-8a09-419cb64b0054';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/br.svg' WHERE ""Id"" = 'b55919aa-b929-4b94-a91b-7e659fa1dd1f';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/ma.svg' WHERE ""Id"" = 'c646d53c-7ea3-45fe-bd95-2a64c2d2f6fd';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/ht.svg' WHERE ""Id"" = '19b2fed0-cce9-4ef8-889f-7926da9f687e';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/gb-sct.svg' WHERE ""Id"" = 'f499eb89-5eaa-4049-88cc-c5a1beca7ce9';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/us.svg' WHERE ""Id"" = '6411deb4-8353-47a8-bb67-81d30475cd3c';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/py.svg' WHERE ""Id"" = 'c80a79cf-13b8-471a-802b-6dfd2ddadbc2';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/au.svg' WHERE ""Id"" = '8dd245e0-126f-4b8c-b2f7-fa2219e05f6f';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/tr.svg' WHERE ""Id"" = '7ce211e1-3171-441a-9a1d-7b917ce2c9a3';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/de.svg' WHERE ""Id"" = '84007b03-1ace-4e06-abcf-714601ee4e32';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/cw.svg' WHERE ""Id"" = 'd8cb4b95-2613-4675-a636-ba8b22c19b6a';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/ec.svg' WHERE ""Id"" = 'e335eacc-e35a-43ca-965f-4787729a7a22';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/ci.svg' WHERE ""Id"" = 'b7db59e1-e94d-418a-a245-6561892f58f6';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/nl.svg' WHERE ""Id"" = '0c0bf7cc-91d6-4ec2-acbf-47cb340e5ef0';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/jp.svg' WHERE ""Id"" = '86a61883-bb71-499b-9d41-0bfd776bc8fa';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/se.svg' WHERE ""Id"" = '0db4264e-5dc0-4955-a6f0-278cf25c5c3c';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/tn.svg' WHERE ""Id"" = '4ccc5ff6-30fd-4a2b-980a-4f87df3fb214';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/be.svg' WHERE ""Id"" = 'a6c89039-8607-468e-ad54-c5bfce144700';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/eg.svg' WHERE ""Id"" = '72f05bcf-1325-4786-91db-e9892d31f977';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/ir.svg' WHERE ""Id"" = '35d82265-23a6-4b6b-8f84-4be17b5d8eb7';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/nz.svg' WHERE ""Id"" = 'fddf2d26-aa8e-4722-a3b2-afb57c54fed5';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/es.svg' WHERE ""Id"" = 'ad1104d5-7893-4e3b-93db-c618ae8a9820';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/cv.svg' WHERE ""Id"" = '41eb6a14-a42c-44dc-8382-05d79ea27799';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/sa.svg' WHERE ""Id"" = 'b473b215-6635-457c-a55d-a14da9a9a15c';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/uy.svg' WHERE ""Id"" = '88bade79-852b-4fdd-853c-a9a05e444029';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/fr.svg' WHERE ""Id"" = 'c7c59849-4129-4df1-996e-457e5845f76f';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/sn.svg' WHERE ""Id"" = 'c67ec53e-4be0-431a-ac36-0c30bca1c825';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/iq.svg' WHERE ""Id"" = 'dceddcbd-9f59-4ccf-8a0d-dab342bff11e';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/no.svg' WHERE ""Id"" = 'f1a66afb-984f-4f55-be1a-058e36e2ca35';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/ar.svg' WHERE ""Id"" = 'e45010cc-acc7-45c0-85f7-a2dfa998e4cc';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/dz.svg' WHERE ""Id"" = '92d59be1-6e8b-47a4-b171-29a043bb9604';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/at.svg' WHERE ""Id"" = '5c694508-d7c2-46cd-8952-9fdd95845441';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/jo.svg' WHERE ""Id"" = 'c1284e09-64c2-48b8-b352-81bde02bffdd';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/pt.svg' WHERE ""Id"" = 'd6298c4c-4d27-4834-ba6b-ac0e4ac62672';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/cd.svg' WHERE ""Id"" = 'f0f0b509-c5d6-425a-b262-171cc2db106f';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/uz.svg' WHERE ""Id"" = '8fab77ca-ea90-4fca-b64a-1772c4f8f8c9';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/co.svg' WHERE ""Id"" = 'dca087fb-f7aa-4a92-9998-d5288a47de3b';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/gb-eng.svg' WHERE ""Id"" = 'bb0e6c60-9f0e-4e00-a6ed-d5dedb8b1ddc';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/hr.svg' WHERE ""Id"" = '389cc346-595a-4a67-ba34-d25137c70302';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/gh.svg' WHERE ""Id"" = 'f4389ea3-1ead-423e-9e63-37387dc1f25b';
UPDATE ""Teams"" SET ""FlagUrl"" = 'https://flagcdn.com/pa.svg' WHERE ""Id"" = '80ed2731-a65b-4437-af7e-4b927676b6ef';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""Teams"" SET ""FlagUrl"" = NULL;");
        }
    }
}
