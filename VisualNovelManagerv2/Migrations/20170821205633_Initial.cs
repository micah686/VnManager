using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace VisualNovelManagerv2.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnCharacterVns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReleaseId = table.Column<int>(type: "INTEGER", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    SpoilerLevel = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnCharacterVns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoAnime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AniDbId = table.Column<int>(type: "INTEGER", nullable: true),
                    AniNfoId = table.Column<string>(type: "TEXT", nullable: true),
                    AnimeType = table.Column<string>(type: "TEXT", nullable: true),
                    AnnId = table.Column<int>(type: "INTEGER", nullable: true),
                    TitleEng = table.Column<string>(type: "TEXT", nullable: true),
                    TitleJpn = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true),
                    Year = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfoAnime", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Encubed = table.Column<string>(type: "TEXT", nullable: true),
                    Renai = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true),
                    Wikipedia = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfoLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Official = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    Relation = table.Column<string>(type: "TEXT", nullable: true),
                    RelationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfoRelations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoStaff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AliasId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    StaffId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfoStaff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnProducerLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Homepage = table.Column<string>(type: "TEXT", nullable: true),
                    ProducerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Wikipedia = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnProducerLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnProducerRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    ProducerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Relation = table.Column<string>(type: "TEXT", nullable: true),
                    RelationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnProducerRelations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnReleaseMedia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Medium = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: true),
                    ReleaseId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnReleaseMedia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnReleaseProducers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Developer = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    ProducerId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProducerType = table.Column<string>(type: "TEXT", nullable: true),
                    Publisher = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnReleaseProducers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnReleaseVn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnId = table.Column<uint>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnReleaseVn", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnStaffAliases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AliasId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    StaffId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnStaffAliases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnStaffLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AniDb = table.Column<string>(type: "TEXT", nullable: true),
                    Homepage = table.Column<string>(type: "TEXT", nullable: true),
                    StaffId = table.Column<int>(type: "INTEGER", nullable: true),
                    Twitter = table.Column<string>(type: "TEXT", nullable: true),
                    Wikipedia = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnStaffLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnTagData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Aliases = table.Column<string>(type: "TEXT", nullable: true),
                    Cat = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Meta = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Parents = table.Column<string>(type: "TEXT", nullable: true),
                    TagId = table.Column<int>(type: "INTEGER", nullable: true),
                    Vns = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnTagData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnTraitData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Aliases = table.Column<string>(type: "TEXT", nullable: true),
                    Chars = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Meta = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Parents = table.Column<string>(type: "TEXT", nullable: true),
                    TraitId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnTraitData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnUserData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExePath = table.Column<string>(type: "TEXT", nullable: true),
                    IconPath = table.Column<string>(type: "TEXT", nullable: true),
                    LastPlayed = table.Column<string>(type: "TEXT", nullable: true),
                    PlayTime = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<uint>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnUserData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnUserDataCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<int>(type: "INTEGER", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnUserDataCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnVisualNovelList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Added = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    VnId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnVisualNovelList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnVoteList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Added = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    VnId = table.Column<int>(type: "INTEGER", nullable: false),
                    Vote = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnVoteList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnWishList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Added = table.Column<string>(type: "TEXT", nullable: true),
                    Priority = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    VnId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnWishList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnProducer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Aliases = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    ProducerId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProducerType = table.Column<string>(type: "TEXT", nullable: true),
                    VnProducerLinksId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnProducerRelationsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnProducer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VnProducer_VnProducerLinks_VnProducerLinksId",
                        column: x => x.VnProducerLinksId,
                        principalTable: "VnProducerLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnProducer_VnProducerRelations_VnProducerRelationsId",
                        column: x => x.VnProducerRelationsId,
                        principalTable: "VnProducerRelations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VnStaff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    MainAlias = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    StaffId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnStaffAliasesId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnStaffLinksId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VnStaff_VnStaffAliases_VnStaffAliasesId",
                        column: x => x.VnStaffAliasesId,
                        principalTable: "VnStaffAliases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnStaff_VnStaffLinks_VnStaffLinksId",
                        column: x => x.VnStaffLinksId,
                        principalTable: "VnStaffLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VnCharacterTraits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: true),
                    SpoilerLevel = table.Column<string>(type: "TEXT", nullable: true),
                    TraitId = table.Column<int>(type: "INTEGER", nullable: true),
                    TraitName = table.Column<string>(type: "TEXT", nullable: true),
                    VnCharacterId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnCharacterTraits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnCharacter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Aliases = table.Column<string>(type: "TEXT", nullable: true),
                    Birthday = table.Column<string>(type: "TEXT", nullable: true),
                    BloodType = table.Column<string>(type: "TEXT", nullable: true),
                    Bust = table.Column<int>(type: "INTEGER", nullable: true),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    Hip = table.Column<int>(type: "INTEGER", nullable: true),
                    ImageLink = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    VnCharacterVnsId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnId = table.Column<uint>(type: "INTEGER", nullable: true),
                    VnInfoId = table.Column<int>(type: "INTEGER", nullable: true),
                    Waist = table.Column<int>(type: "INTEGER", nullable: true),
                    Weight = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnCharacter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VnCharacter_VnCharacterVns_VnCharacterVnsId",
                        column: x => x.VnCharacterVnsId,
                        principalTable: "VnCharacterVns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoScreens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Nsfw = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseId = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoId = table.Column<int>(type: "INTEGER", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfoScreens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VnInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Aliases = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ImageLink = table.Column<string>(type: "TEXT", nullable: true),
                    ImageNsfw = table.Column<string>(type: "TEXT", nullable: true),
                    Languages = table.Column<string>(type: "TEXT", nullable: true),
                    Length = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalLanguage = table.Column<string>(type: "TEXT", nullable: true),
                    Platforms = table.Column<string>(type: "TEXT", nullable: true),
                    Popularity = table.Column<double>(type: "REAL", nullable: true),
                    Rating = table.Column<int>(type: "INTEGER", nullable: true),
                    Released = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: false),
                    VnInfoAnimeId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoLinksId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoRelationsId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoScreensId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoStaffId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VnInfo_VnInfoAnime_VnInfoAnimeId",
                        column: x => x.VnInfoAnimeId,
                        principalTable: "VnInfoAnime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnInfo_VnInfoLinks_VnInfoLinksId",
                        column: x => x.VnInfoLinksId,
                        principalTable: "VnInfoLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnInfo_VnInfoRelations_VnInfoRelationsId",
                        column: x => x.VnInfoRelationsId,
                        principalTable: "VnInfoRelations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnInfo_VnInfoScreens_VnInfoScreensId",
                        column: x => x.VnInfoScreensId,
                        principalTable: "VnInfoScreens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnInfo_VnInfoStaff_VnInfoStaffId",
                        column: x => x.VnInfoStaffId,
                        principalTable: "VnInfoStaff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    Spoiler = table.Column<string>(type: "TEXT", nullable: true),
                    TagId = table.Column<int>(type: "INTEGER", nullable: true),
                    TagName = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfoTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VnInfoTags_VnInfo_VnInfoId",
                        column: x => x.VnInfoId,
                        principalTable: "VnInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VnRelease",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Animation = table.Column<string>(type: "TEXT", nullable: true),
                    Catalog = table.Column<string>(type: "TEXT", nullable: true),
                    Doujin = table.Column<string>(type: "TEXT", nullable: true),
                    Freeware = table.Column<string>(type: "TEXT", nullable: true),
                    Gtin = table.Column<string>(type: "TEXT", nullable: true),
                    Languages = table.Column<string>(type: "TEXT", nullable: true),
                    MinAge = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    Patch = table.Column<string>(type: "TEXT", nullable: true),
                    Platforms = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReleaseType = table.Column<string>(type: "TEXT", nullable: true),
                    Released = table.Column<string>(type: "TEXT", nullable: true),
                    Resolution = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<uint>(type: "INTEGER", nullable: true),
                    VnInfoId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnReleaseMediaId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnReleaseProducersId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnReleaseVnId = table.Column<int>(type: "INTEGER", nullable: true),
                    Voiced = table.Column<string>(type: "TEXT", nullable: true),
                    Website = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnRelease", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VnRelease_VnInfo_VnInfoId",
                        column: x => x.VnInfoId,
                        principalTable: "VnInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnRelease_VnReleaseMedia_VnReleaseMediaId",
                        column: x => x.VnReleaseMediaId,
                        principalTable: "VnReleaseMedia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnRelease_VnReleaseProducers_VnReleaseProducersId",
                        column: x => x.VnReleaseProducersId,
                        principalTable: "VnReleaseProducers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnRelease_VnReleaseVn_VnReleaseVnId",
                        column: x => x.VnReleaseVnId,
                        principalTable: "VnReleaseVn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VnCharacter_VnCharacterVnsId",
                table: "VnCharacter",
                column: "VnCharacterVnsId");

            migrationBuilder.CreateIndex(
                name: "IX_VnCharacter_VnInfoId",
                table: "VnCharacter",
                column: "VnInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_VnCharacterTraits_VnCharacterId",
                table: "VnCharacterTraits",
                column: "VnCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_VnInfo_VnInfoAnimeId",
                table: "VnInfo",
                column: "VnInfoAnimeId");

            migrationBuilder.CreateIndex(
                name: "IX_VnInfo_VnInfoLinksId",
                table: "VnInfo",
                column: "VnInfoLinksId");

            migrationBuilder.CreateIndex(
                name: "IX_VnInfo_VnInfoRelationsId",
                table: "VnInfo",
                column: "VnInfoRelationsId");

            migrationBuilder.CreateIndex(
                name: "IX_VnInfo_VnInfoScreensId",
                table: "VnInfo",
                column: "VnInfoScreensId");

            migrationBuilder.CreateIndex(
                name: "IX_VnInfo_VnInfoStaffId",
                table: "VnInfo",
                column: "VnInfoStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_VnInfoScreens_VnInfoId",
                table: "VnInfoScreens",
                column: "VnInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_VnInfoTags_VnInfoId",
                table: "VnInfoTags",
                column: "VnInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_VnProducer_VnProducerLinksId",
                table: "VnProducer",
                column: "VnProducerLinksId");

            migrationBuilder.CreateIndex(
                name: "IX_VnProducer_VnProducerRelationsId",
                table: "VnProducer",
                column: "VnProducerRelationsId");

            migrationBuilder.CreateIndex(
                name: "IX_VnRelease_VnInfoId",
                table: "VnRelease",
                column: "VnInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_VnRelease_VnReleaseMediaId",
                table: "VnRelease",
                column: "VnReleaseMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_VnRelease_VnReleaseProducersId",
                table: "VnRelease",
                column: "VnReleaseProducersId");

            migrationBuilder.CreateIndex(
                name: "IX_VnRelease_VnReleaseVnId",
                table: "VnRelease",
                column: "VnReleaseVnId");

            migrationBuilder.CreateIndex(
                name: "IX_VnStaff_VnStaffAliasesId",
                table: "VnStaff",
                column: "VnStaffAliasesId");

            migrationBuilder.CreateIndex(
                name: "IX_VnStaff_VnStaffLinksId",
                table: "VnStaff",
                column: "VnStaffLinksId");

            migrationBuilder.AddForeignKey(
                name: "FK_VnCharacterTraits_VnCharacter_VnCharacterId",
                table: "VnCharacterTraits",
                column: "VnCharacterId",
                principalTable: "VnCharacter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VnCharacter_VnInfo_VnInfoId",
                table: "VnCharacter",
                column: "VnInfoId",
                principalTable: "VnInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VnInfoScreens_VnInfo_VnInfoId",
                table: "VnInfoScreens",
                column: "VnInfoId",
                principalTable: "VnInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VnInfoScreens_VnInfo_VnInfoId",
                table: "VnInfoScreens");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "VnCharacterTraits");

            migrationBuilder.DropTable(
                name: "VnInfoTags");

            migrationBuilder.DropTable(
                name: "VnProducer");

            migrationBuilder.DropTable(
                name: "VnRelease");

            migrationBuilder.DropTable(
                name: "VnStaff");

            migrationBuilder.DropTable(
                name: "VnTagData");

            migrationBuilder.DropTable(
                name: "VnTraitData");

            migrationBuilder.DropTable(
                name: "VnUserData");

            migrationBuilder.DropTable(
                name: "VnUserDataCategories");

            migrationBuilder.DropTable(
                name: "VnVisualNovelList");

            migrationBuilder.DropTable(
                name: "VnVoteList");

            migrationBuilder.DropTable(
                name: "VnWishList");

            migrationBuilder.DropTable(
                name: "VnCharacter");

            migrationBuilder.DropTable(
                name: "VnProducerLinks");

            migrationBuilder.DropTable(
                name: "VnProducerRelations");

            migrationBuilder.DropTable(
                name: "VnReleaseMedia");

            migrationBuilder.DropTable(
                name: "VnReleaseProducers");

            migrationBuilder.DropTable(
                name: "VnReleaseVn");

            migrationBuilder.DropTable(
                name: "VnStaffAliases");

            migrationBuilder.DropTable(
                name: "VnStaffLinks");

            migrationBuilder.DropTable(
                name: "VnCharacterVns");

            migrationBuilder.DropTable(
                name: "VnInfo");

            migrationBuilder.DropTable(
                name: "VnInfoAnime");

            migrationBuilder.DropTable(
                name: "VnInfoLinks");

            migrationBuilder.DropTable(
                name: "VnInfoRelations");

            migrationBuilder.DropTable(
                name: "VnInfoScreens");

            migrationBuilder.DropTable(
                name: "VnInfoStaff");
        }
    }
}
