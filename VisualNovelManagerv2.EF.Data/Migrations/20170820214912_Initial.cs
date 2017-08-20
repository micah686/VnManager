using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace VisualNovelManagerv2.EF.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnInfo",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    VnId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfo", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnProducer",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Aliases = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    ProducerId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProducerType = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnProducer", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnReleaseMedia",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Medium = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: true),
                    ReleaseId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnReleaseMedia", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnReleaseProducers",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    table.PrimaryKey("PK_VnReleaseProducers", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnReleaseVn",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnReleaseVn", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnStaff",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    MainAlias = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    StaffId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnStaff", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnTagData",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    table.PrimaryKey("PK_VnTagData", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnTraitData",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    table.PrimaryKey("PK_VnTraitData", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnUserData",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExePath = table.Column<string>(type: "TEXT", nullable: true),
                    IconPath = table.Column<string>(type: "TEXT", nullable: true),
                    LastPlayed = table.Column<string>(type: "TEXT", nullable: true),
                    PlayTime = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnUserData", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnUserDataCategories",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<int>(type: "INTEGER", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnUserDataCategories", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnVisualNovelList",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Added = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    VnId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnVisualNovelList", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnVoteList",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Added = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    VnId = table.Column<int>(type: "INTEGER", nullable: false),
                    Vote = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnVoteList", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnWishList",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Added = table.Column<string>(type: "TEXT", nullable: true),
                    Priority = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    VnId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnWishList", x => x.PkId);
                });

            migrationBuilder.CreateTable(
                name: "VnCharacter",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    VnId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoPkId = table.Column<int>(type: "INTEGER", nullable: true),
                    Waist = table.Column<int>(type: "INTEGER", nullable: true),
                    Weight = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnCharacter", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnCharacter_VnInfo_VnInfoPkId",
                        column: x => x.VnInfoPkId,
                        principalTable: "VnInfo",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoAnime",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_VnInfoAnime", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnInfoAnime_VnInfo_PkId",
                        column: x => x.PkId,
                        principalTable: "VnInfo",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoLinks",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false),
                    Encubed = table.Column<string>(type: "TEXT", nullable: true),
                    Renai = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true),
                    Wikipedia = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfoLinks", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnInfoLinks_VnInfo_PkId",
                        column: x => x.PkId,
                        principalTable: "VnInfo",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoRelations",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false),
                    Official = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    Relation = table.Column<string>(type: "TEXT", nullable: true),
                    RelationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfoRelations", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnInfoRelations_VnInfo_PkId",
                        column: x => x.PkId,
                        principalTable: "VnInfo",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoScreens",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Nsfw = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseId = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoPkId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoPkId1 = table.Column<int>(type: "INTEGER", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfoScreens", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnInfoScreens_VnInfo_VnInfoPkId",
                        column: x => x.VnInfoPkId,
                        principalTable: "VnInfo",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnInfoScreens_VnInfo_VnInfoPkId1",
                        column: x => x.VnInfoPkId1,
                        principalTable: "VnInfo",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoStaff",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_VnInfoStaff", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnInfoStaff_VnInfo_PkId",
                        column: x => x.PkId,
                        principalTable: "VnInfo",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VnInfoTags",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    Spoiler = table.Column<string>(type: "TEXT", nullable: true),
                    TagId = table.Column<int>(type: "INTEGER", nullable: true),
                    TagName = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoPkId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnInfoTags", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnInfoTags_VnInfo_VnInfoPkId",
                        column: x => x.VnInfoPkId,
                        principalTable: "VnInfo",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VnProducerLinks",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false),
                    Homepage = table.Column<string>(type: "TEXT", nullable: true),
                    ProducerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Wikipedia = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnProducerLinks", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnProducerLinks_VnProducer_PkId",
                        column: x => x.PkId,
                        principalTable: "VnProducer",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VnProducerRelations",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    ProducerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Relation = table.Column<string>(type: "TEXT", nullable: true),
                    RelationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnProducerRelations", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnProducerRelations_VnProducer_PkId",
                        column: x => x.PkId,
                        principalTable: "VnProducer",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VnRelease",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    VnId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnInfoPkId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnReleaseMediaPkId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnReleaseProducersPkId = table.Column<int>(type: "INTEGER", nullable: true),
                    VnReleaseVnPkId = table.Column<int>(type: "INTEGER", nullable: true),
                    Voiced = table.Column<string>(type: "TEXT", nullable: true),
                    Website = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnRelease", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnRelease_VnInfo_VnInfoPkId",
                        column: x => x.VnInfoPkId,
                        principalTable: "VnInfo",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnRelease_VnReleaseMedia_VnReleaseMediaPkId",
                        column: x => x.VnReleaseMediaPkId,
                        principalTable: "VnReleaseMedia",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnRelease_VnReleaseProducers_VnReleaseProducersPkId",
                        column: x => x.VnReleaseProducersPkId,
                        principalTable: "VnReleaseProducers",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VnRelease_VnReleaseVn_VnReleaseVnPkId",
                        column: x => x.VnReleaseVnPkId,
                        principalTable: "VnReleaseVn",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VnStaffAliases",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false),
                    AliasId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Original = table.Column<string>(type: "TEXT", nullable: true),
                    StaffId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnStaffAliases", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnStaffAliases_VnStaff_PkId",
                        column: x => x.PkId,
                        principalTable: "VnStaff",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VnStaffLinks",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false),
                    AniDb = table.Column<string>(type: "TEXT", nullable: true),
                    Homepage = table.Column<string>(type: "TEXT", nullable: true),
                    StaffId = table.Column<int>(type: "INTEGER", nullable: true),
                    Twitter = table.Column<string>(type: "TEXT", nullable: true),
                    Wikipedia = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnStaffLinks", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnStaffLinks_VnStaff_PkId",
                        column: x => x.PkId,
                        principalTable: "VnStaff",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VnCharacterTraits",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: true),
                    SpoilerLevel = table.Column<string>(type: "TEXT", nullable: true),
                    TraitId = table.Column<int>(type: "INTEGER", nullable: true),
                    TraitName = table.Column<string>(type: "TEXT", nullable: true),
                    VnCharacterPkId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnCharacterTraits", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnCharacterTraits_VnCharacter_VnCharacterPkId",
                        column: x => x.VnCharacterPkId,
                        principalTable: "VnCharacter",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VnCharacterVns",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReleaseId = table.Column<int>(type: "INTEGER", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    SpoilerLevel = table.Column<string>(type: "TEXT", nullable: true),
                    VnId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnCharacterVns", x => x.PkId);
                    table.ForeignKey(
                        name: "FK_VnCharacterVns_VnCharacter_PkId",
                        column: x => x.PkId,
                        principalTable: "VnCharacter",
                        principalColumn: "PkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VnCharacter_VnInfoPkId",
                table: "VnCharacter",
                column: "VnInfoPkId");

            migrationBuilder.CreateIndex(
                name: "IX_VnCharacterTraits_VnCharacterPkId",
                table: "VnCharacterTraits",
                column: "VnCharacterPkId");

            migrationBuilder.CreateIndex(
                name: "IX_VnInfoScreens_VnInfoPkId",
                table: "VnInfoScreens",
                column: "VnInfoPkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VnInfoScreens_VnInfoPkId1",
                table: "VnInfoScreens",
                column: "VnInfoPkId1");

            migrationBuilder.CreateIndex(
                name: "IX_VnInfoTags_VnInfoPkId",
                table: "VnInfoTags",
                column: "VnInfoPkId");

            migrationBuilder.CreateIndex(
                name: "IX_VnRelease_VnInfoPkId",
                table: "VnRelease",
                column: "VnInfoPkId");

            migrationBuilder.CreateIndex(
                name: "IX_VnRelease_VnReleaseMediaPkId",
                table: "VnRelease",
                column: "VnReleaseMediaPkId");

            migrationBuilder.CreateIndex(
                name: "IX_VnRelease_VnReleaseProducersPkId",
                table: "VnRelease",
                column: "VnReleaseProducersPkId");

            migrationBuilder.CreateIndex(
                name: "IX_VnRelease_VnReleaseVnPkId",
                table: "VnRelease",
                column: "VnReleaseVnPkId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "VnCharacterTraits");

            migrationBuilder.DropTable(
                name: "VnCharacterVns");

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

            migrationBuilder.DropTable(
                name: "VnInfoTags");

            migrationBuilder.DropTable(
                name: "VnProducerLinks");

            migrationBuilder.DropTable(
                name: "VnProducerRelations");

            migrationBuilder.DropTable(
                name: "VnRelease");

            migrationBuilder.DropTable(
                name: "VnStaffAliases");

            migrationBuilder.DropTable(
                name: "VnStaffLinks");

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
                name: "VnProducer");

            migrationBuilder.DropTable(
                name: "VnReleaseMedia");

            migrationBuilder.DropTable(
                name: "VnReleaseProducers");

            migrationBuilder.DropTable(
                name: "VnReleaseVn");

            migrationBuilder.DropTable(
                name: "VnStaff");

            migrationBuilder.DropTable(
                name: "VnInfo");
        }
    }
}
