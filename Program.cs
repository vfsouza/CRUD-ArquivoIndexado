using System;
using Trabalho2.Models;

namespace Trabalho2 {
	class Program {
		static void Main(string[] args) {
			FileHandler fh = new FileHandler();
			List<ContaBancaria> list = new List<ContaBancaria>();
			fh.BuildFile();
			fh.CreateInvertedFiles();

			// Loop para escolher as opções.
			while (true) {
				Console.Clear();
				switch (Menu()) {

					// Criar conta
					case 1: {
						LineWrap();
						Console.WriteLine("ABERTURA DE CONTA\n");

						Console.Write("Nome da conta..> ");
						string? nomePessoa = Console.ReadLine();

						Console.Write("CPF da conta...> ");
						string? cPF = Console.ReadLine();

						Console.Write("Cidada da conta> ");
						string? cidade = Console.ReadLine();
						ContaBancaria contaNova = new ContaBancaria();

						if (nomePessoa != null && cPF != null && cidade != null) {
							fh.Create(contaNova);

							Console.WriteLine("\nConta criada com sucesso!\n");
							Console.WriteLine(contaNova.ToString());
							Thread.Sleep(5000);
						} else {
							Console.WriteLine("\nFalta de informações para criar conta!\n");
							Thread.Sleep(2000);
						}

						
						break;
					}

					// Depositar
					case 2: {
						LineWrap();
						Console.Write("ID da sua conta> ");
						ushort id = ushort.Parse(Console.ReadLine());

						ContaBancaria? conta = fh.ReadById(id);

						if (conta != null) {
							LineWrap();
							Console.Write("Quantia a ser depositada> ");
							conta.Depositar(float.Parse(Console.ReadLine()));

							fh.UpdateById(conta, id);
						}
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

						ContaBancaria? conta1 = fh.ReadById(id1);
						ContaBancaria? conta2 = fh.ReadById(id2);

						if (conta1 != null && conta2 != null) {
							if (conta1.SaldoConta >= transf) {
								conta1.Transferir(transf);
								conta1.TransfRealizadas++;
							}

							conta2.Depositar(transf);

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
						Console.Write("Deseja fazer a busca por Cidade(0), Nome(1) ou ID(2)?> ");
						ushort? op = ushort.Parse(Console.ReadLine());

						if (op == null) {
							Console.WriteLine("\nOperação não encontrada!\n");
							Thread.Sleep(2000);
							break;
						}

						switch (op) {
							case 0:
								Console.Write("Digite o nome da cidade> ");
								string? cidade = Console.ReadLine();

								if (cidade == null) {
									Console.WriteLine("\nOperação não encontrada!\n");
									Thread.Sleep(2000);
									break;
								}

								list = fh.ReadByCity(cidade);
								for (int i = 0; i < list.Count; i++) {
									if (!list.ElementAt(i).Lapide) {
										Console.WriteLine("\n" + list.ElementAt(i));
									} else if (list.ElementAt(i) == null) {
										Console.WriteLine("\nConta não existe!");
									} else {
										Console.WriteLine("\nConta foi excluída!");
									}
								}
								break;

							case 1:
								Console.Write("Digite o nome da conta> ");
								string? nome = Console.ReadLine();

								if (nome == null) {
									Console.WriteLine("\nOperação não encontrada!\n");
									Thread.Sleep(2000);
									break;
								}

								list = fh.ReadByName(nome);
								for (int i = 0; i < list.Count; i++) {
									if (!list.ElementAt(i).Lapide) {
										Console.WriteLine("\n" + list.ElementAt(i));
									} else if (list.ElementAt(i) == null) {
										Console.WriteLine("\nConta não existe!");
									} else {
										Console.WriteLine("\nConta foi excluída!");
									}
								}
								break;

							case 2:
								Console.Write("Digite o ID da conta> ");
								ushort? id = ushort.Parse(Console.ReadLine());
								long pos = fh.FindPosByIndex(id.GetValueOrDefault());

								ContaBancaria? conta = fh.ReadByPos(pos);

								if (conta != null) {
									if (!conta.Lapide) {
										Console.WriteLine("\n" + conta);
									} else {
										Console.WriteLine("\nConta foi excluída!");
									}
								} else {
									Console.WriteLine("\nConta não existe!");
								}
								break;

							default:
								Console.WriteLine("Opção inválida!");
								break;
						}
						Thread.Sleep(5000);
						break;
					}

					// Atualizar registro
					case 5: {
						LineWrap();
						Console.Write("Qual ID da conta você deseja atualizar?> ");
						ushort id = ushort.Parse(Console.ReadLine());

						ContaBancaria? conta = fh.ReadById(id);

						if (conta == null) {
							Console.WriteLine("\nO ID informado não pertence a nenhuma conta!\n");
							Thread.Sleep(4000);
							break;
						}

						Console.Write("Nome da conta..> ");
						string nome = Console.ReadLine();

						Console.Write("CPF da conta...> ");
						string cPF = Console.ReadLine();

						Console.Write("Cidade da conta> ");
						string cidade = Console.ReadLine();

						if (nome != null && cPF != null && cidade != null) {
							Console.WriteLine("\nAntes da atualização:\n");
							Console.WriteLine(conta.ToString());

							conta.NomePessoa = nome; conta.CPF = cpf; conta.Cidade = cidade;
							fh.UpdateById(conta, id);

							Console.WriteLine("\nDepois da atualização:\n");
							Console.WriteLine(conta.ToString());
							Thread.Sleep(5000);
						} else {
							Console.WriteLine("\nFalta de informações para criar conta!\n");
							Thread.Sleep(2000);
						}

						
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