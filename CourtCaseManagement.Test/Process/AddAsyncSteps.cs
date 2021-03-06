﻿using CourtCaseManagement.ApplicationCore.Entities;
using CourtCaseManagement.ApplicationCore.TOs;
using CourtCaseManagement.Test.Scenario;
using CourtCaseManagement.Test.Scenario.Helpers;
using CourtCaseManagement.Test.Scenario.WebApi;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Xunit;

namespace CourtCaseManagement.Test.Process
{
    [Binding]
    internal class AddAsyncSteps : BaseTest, IClassFixture<TestServerFixture>
    {
        public AddAsyncSteps(TestServerFixture factory) : base(factory)
        {
            
        }

        [BeforeScenario("@ProcessAddAsync")]
        public void BeforeScenario() => BaseBeforeScenario();

        [Given(@"um funcionario cadastrando um novo processo (.*)")]
        public void DadoUmFuncionarioCadastrandoUmNovoProcesso(string unifiedProcessNumber)
        {
            Variable.UnifiedProcessNumber = unifiedProcessNumber.Replace("\"", string.Empty);
        }
        
        [Given(@"com a data de distribuição (.*)")]
        public void DadoComADataDeDistribuicao(string distributionDate)
        {
            Variable.DistributionDate = distributionDate.Replace("\"", string.Empty);
        }
        
        [Given(@"com o processo segredo de justiça (.*)")]
        public void DadoComOProcessoSegredoDeJustica(string justiceSecret)
        {
            Variable.JusticeSecret = justiceSecret.Replace("\"", string.Empty);
        }
        
        [Given(@"com a pasta física cliente (.*)")]
        public void DadoComAPastaFisicaCliente(string clientPhysicalFolder)
        {
            Variable.ClientPhysicalFolder = clientPhysicalFolder.Replace("\"", string.Empty);
        }
        
        [Given(@"com a descrição (.*)")]
        public void DadoComADescricao(string description)
        {
            Variable.Description = description.Replace("\"", string.Empty);
        }
        
        [Given(@"com a situação (.*)")]
        public void DadoComASituacao(string situationId)
        {
            Variable.SituationId = situationId.Replace("\"", string.Empty);
        }

        [Given(@"com o cpf do responsável (.*)")]
        public void DadoComOCpfDoResponsavel(string responsiblesCpf)
        {
            Variable.ResponsiblesCpf = responsiblesCpf.Replace("\"", string.Empty);
        }

        [Given(@"com o nome do responsável (.*)")]
        public void DadoComONomeDoResponsavel(string responsiblesName)
        {
            Variable.ResponsiblesName = responsiblesName.Replace("\"", string.Empty);
        }

        [Given(@"com o e-mail do responsável (.*)")]
        public void DadoComOE_MailDoResponsavel(string responsiblesEMail)
        {
            Variable.ResponsiblesEMail = responsiblesEMail.Replace("\"", string.Empty);
        }

        [Given(@"com a foto do responsável (.*)")]
        public void DadoComAFotoDoResponsavel(string image)
        {
            Variable.Image = image.Replace("\"", string.Empty);
        }

        [When(@"solicitar o cadastro do processo")]
        public async Task QuandoSolicitarOCadastroDoProcesso()
        {
            Variable.ProcessResponseTO = await HttpClientHelper.PostAsync<ProcessResponseTO>($"/courtCaseManagement/v1/process", await CreateProcess());
        }

        [Then(@"o sistema retornara o codigo HTML de criado com sucesso")]
        public void EntaoOSistemaRetornaraOCodigoHTMLDeCriadoComSucesso()
        {
            Assert.Equal(HttpStatusCode.Created, Variable.StatusCode);
        }

        [Then(@"apresentara os seguintes codigo de erros ""(.*)""")]
        public void EntaoApresentaraOsSeguintesCodigoDeErros(string messages)
        {
            var listMsg = messages.Split(";").ToList();
            Variable.ErrorsResponse.Errors.ForEach(error => Assert.Contains(error.Validation, listMsg));
        }

        [Then(@"o sistema retornara o código (.*)")]
        public void EntaoOSistemaRetornaraOCodigo(int statusCode)
        {
            Assert.Equal(statusCode, (int)Variable.StatusCode);
        }

        private async Task<ProcessRequestTO> CreateProcess()
        {
            Guid? situationId = null;
            if (!string.IsNullOrEmpty(Variable.SituationId))
            {
                var situationRepository = Repository.CatalogContext.Set<SituationEntity>();
                situationId = ((await situationRepository.ToListAsync()).First(situation => situation.Name.Trim().ToUpper() == Variable.SituationId.Trim().ToUpper())).Id;
            }

            var responsibleRepository = Repository.CatalogContext.Set<ResponsibleEntity>();
            ResponsibleEntity responsibleEntity = null;            
            if (!string.IsNullOrEmpty(Variable.ResponsiblesCpf) && !string.IsNullOrEmpty(Variable.ResponsiblesName) && !string.IsNullOrEmpty(Variable.ResponsiblesEMail) && !string.IsNullOrEmpty(Variable.Image))
            {
                responsibleEntity = (await responsibleRepository.AddAsync(new ResponsibleEntity
                {
                    Cpf = Convert.ToInt64(Variable.ResponsiblesCpf.Replace(".", "").Replace("-", "")),
                    Name = Variable.ResponsiblesName,
                    Mail = Variable.ResponsiblesEMail,
                    Photograph = Variable.Image,
                })).Entity;
            }

            var processRequestTO = new ProcessRequestTO
            {
                Description = Variable.Description,
                UpdateUserName = "teste",
                DistributionDate = !string.IsNullOrEmpty(Variable.DistributionDate) ? (DateTime?)DateTime.ParseExact(Variable.DistributionDate, "yyyy-MM-dd", CultureInfo.CreateSpecificCulture("pt-BR")) : null,
                JusticeSecret = !string.IsNullOrEmpty(Variable.JusticeSecret) ? (bool?)(Variable.JusticeSecret == "S" ? true : false) : null,
                SituationId = situationId,
                UnifiedProcessNumber = Variable.UnifiedProcessNumber,
                ClientPhysicalFolder = Variable.ClientPhysicalFolder,
                Responsibles = responsibleEntity != null ? new List<Guid?>
                {
                    responsibleEntity.Id
                } : null
            };

            await Repository.CatalogContext.SaveChangesAsync();

            return processRequestTO;
        }
    }
}