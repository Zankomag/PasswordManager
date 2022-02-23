﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PasswordManager.Infrastructure.Data;

namespace PasswordManager.Infrastructure.Migrations
{
    [DbContext(typeof(PasswordManagerDbContext))]
    [Migration("20201203201331_Init4")]
    partial class Init4
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("PasswordManager.Core.Entities.Account", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AccountName")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("TEXT");

                    b.Property<bool>("Encrypted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Link")
                        .HasMaxLength(70)
                        .HasColumnType("TEXT");

                    b.Property<string>("Login")
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<string>("Note")
                        .HasMaxLength(512)
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("OutdatedTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .HasMaxLength(2048)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("PasswordUpdatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("PasswordManager.Core.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Action")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PasswordGeneratorPattern")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("KeyHint")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("Lang")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("OutdatedTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PasswordManager.Core.Entities.Account", b =>
                {
                    b.HasOne("PasswordManager.Core.Entities.User", "User")
                        .WithMany("Accounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PasswordManager.Core.Entities.User", b =>
                {
                    b.Navigation("Accounts");
                });
#pragma warning restore 612, 618
        }
    }
}
