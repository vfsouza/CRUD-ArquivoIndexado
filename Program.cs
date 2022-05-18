using System;
using Trabalho1.Models;

namespace Trabalho1 {
	class Program {
		static void Main(string[] args) {
			FileHandler fh = new FileHandler();
			fh.BuildFile();

			// Loop para escolher as opções.
			while (true) {
				Console.Clear();
				switch (Menu()) {

					// Criar conta
					case 1: {
						LineWrap();
						ContaBancaria contaNova = new ContaBancaria();
						Console.WriteLine("ABERTURA DE CONTA\n");

						Console.Write("Nome da conta..> ");
						contaNova.NomePessoa = Console.ReadLine();

						Console.Write("CPF da conta...> ");
						contaNova.CPF = Console.ReadLine();

						Console.Write("Cidada da conta> ");
						contaNova.Cidade = Console.ReadLine();

						fh.Create(contaNova);

						Console.WriteLine("\nConta criada com sucesso!\n");
						Console.WriteLine(contaNova.ToString());
						Thread.Sleep(5000);
						break;
					}

					// Depositar
					case 2: {
						LineWrap();
						Console.Write("ID da sua conta> ");
						ushort id = ushort.Parse(Console.ReadLine());

						ContaBancaria conta = fh.ReadById(id);

						LineWrap();
						Console.Write("Quantia a ser depositada> ");
						conta.SaldoConta += float.Parse(Console.ReadLine());

						fh.UpdateById(conta, id);
						break;
					}

					// Transferir
					case 3: {
						LineWrap();
						Console.Write("ID da sua conta> ");
						ushort id1 = ushort.Parse(Console.ReadLine());

						Console.Write("ID da conta à receber a transferência> ");
						ushort id2 = ushort.Parse(Console.ReadLine());

						Console.Write("Quanto você deseja transferir> ");
						float transf = float.Parse(Console.ReadLine());

						ContaBancaria conta1 = fh.ReadById(id1);
						ContaBancaria conta2 = fh.ReadById(id2);

						if (conta1 != null && conta2 != null) {
							if (conta1.SaldoConta >= transf) {
								conta1.SaldoConta -= transf;
								conta1.TransfRealizadas++;
							}

							conta2.SaldoConta += transf;

							fh.UpdateById(conta1, id1);
							fh.UpdateById(conta2, id2);
						} else {
							Console.WriteLine("\nUma ou todas as contas digitadas não existem!\n");
						}
						break;
					}

					// Ver registro
					case 4: {
						LineWrap();
						Console.Write("Qual ID da conta você deseja ver?> ");
						ushort id = ushort.Parse(Console.ReadLine());

						long pos = fh.FindPosByIndex(id);
						ContaBancaria? conta = fh.ReadByPos(pos);

						if (conta != null) {
							if (!conta.Lapide && conta.NomePessoa != "") {
								Console.WriteLine("\nConta encontrada com sucesso!\n");

								Console.WriteLine(conta.ToString());
							} else if (conta.Lapide) {
								Console.WriteLine("\nConta foi excluída\n");
							}
						} else {
							Console.WriteLine("\nEssa conta não existe\n");
						}
						Thread.Sleep(5000);
						break;
					}

					// Atualizar registro
					case 5: {
						LineWrap();
						Console.Write("Qual ID da conta você deseja atualizar?> ");
						ushort id = ushort.Parse(Console.ReadLine());

						ContaBancaria conta = fh.ReadById(id);

						if (conta == null) {
							Console.WriteLine("\nO ID informado não pertence a nenhuma conta!\n");
							Thread.Sleep(4000);
							break;
						}

						Console.Write("Nome da conta..> ");
						string nome = Console.ReadLine();

						Console.Write("CPF da conta...> ");
						string cpf = Console.ReadLine();

						Console.Write("Cidade da conta> ");
						string cidade = Console.ReadLine();


						Console.WriteLine("\nAntes da atualização:\n");
						Console.WriteLine(conta.ToString());

						conta.NomePessoa = nome; conta.CPF = cpf; conta.Cidade = cidade;
						fh.UpdateById(conta, id);

						Console.WriteLine("\nDepois da atualização:\n");
						Console.WriteLine(conta.ToString());
						Thread.Sleep(5000);
						break;
					}

					// Deletar registro
					case 6: {
						LineWrap();
						Console.Write("Qual ID da conta você deseja deletar?> ");
						ushort id = ushort.Parse(Console.ReadLine());

						if (fh.DeleteById(id))
							Console.WriteLine($"\nConta de ID {id} deletada com sucesso!\n");
						else
							Console.WriteLine($"\nA conta com o ID {id} não existe\n");
						Thread.Sleep(5000);
						break;
					}

					// Sair do programa
					case 0: {
						Console.Clear();
						Console.Write("Fechando programa... ");
						Console.ReadLine();
						break;
					}

					// Opção inválida
					default: {
						Console.Clear();
						Console.WriteLine("\n\nOpção inválida!\n\n");
						Console.Clear();
						break;
					}
				}
			}

		}

		/// <summary>
		/// Imprime o menu de opções na tela.
		/// </summary>
		/// <returns>O número da opção escolhida pelo usuário.</returns>
		public static int Menu() {
			Console.Clear();
			Console.Write("1. Abrir conta\n" +
				"2. Depositar\n" +
				"3. Transferir\n" +
				"4. Ver registro\n" +
				"5. Atualizar registro\n" +
				"6. Deletar registro\n" +
				"0. Sair\n" +
				"\n: ");
			return int.Parse(Console.ReadLine());
		}

		/// <summary>
		/// Imprime um wrapper para separação no CMD.
		/// </summary>
		public static void LineWrap() {
			Console.WriteLine("\n---------------------------------------------\n");
		}
	}
}