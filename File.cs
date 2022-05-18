using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Trabalho1.Environment;
using Trabalho1.Models;

namespace Trabalho1 {
	public class FileHandler {

		// CRUD (Create, Read, Update, Delete) do Banco de Dados de ContaBancaria.

		/// <summary>
		/// Caminho para o arquivo Conta.db.
		/// </summary>
		private string pathContaDB = EnvironmentVar.DBPath;

		/// <summary>
		/// Caminho para o arquivo Index.db.
		/// </summary>
		private string pathIndexDB = EnvironmentVar.IndexPath;

		/// <summary>
		/// Cria um novo registro no Banco de Dados.
		/// </summary>
		/// <param name="conta">Conta a ser inserida no Banco de Dados.</param>
		/// <returns>Um <see cref="Boolean"/>. Se <see langword="true"/>, então o registro foi atualizado com sucesso. Se <see langword="false"/>, o registro não foi atualizado.</returns>
		public bool Create(ContaBancaria conta) {

			long pos;

			if (conta == null) {
				return false;
			}

			// Lê o ID máximo no início do arquivo.
			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Seek(0, SeekOrigin.Begin);
					conta.IdConta = (ushort)(br.ReadUInt16() + 1);
				}

				// Faz o update do ID máximo no início do arquivo.
				UpdateMaxId(conta.IdConta);
			}

			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.Write)) {
				using (BinaryWriter bw = new BinaryWriter(fs)) {
					bw.Seek(0, SeekOrigin.End);
					pos = bw.BaseStream.Position;
					bw.Write(false);
					bw.Write(conta.Serialize().Length);
					bw.Write(conta.Serialize());
				}
			}
			CreateIndex(conta.IdConta, pos);
			return true;
		}

		/// <summary>
		/// Marca a <see cref="ContaBancaria.Lapide"/> como <see langword="true"/>. Ou seja, a conta vai ser indicada como: excluída.
		/// </summary>
		/// <param name="contaId">O ID da conta a ser excluída</param>
		/// <returns><see cref="ContaBancaria"/> que foi deletada.</returns>
		public bool DeleteById(ushort contaId) {
			long pos = 0;
			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.ReadWrite)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Seek(2, SeekOrigin.Begin);

					// Roda um loop até encontrar o ID da conta procurada
					while (br.PeekChar() != -1) {
						pos = br.BaseStream.Position;
						bool lapide = br.ReadBoolean();
						int tam = br.ReadInt32();

						if (!lapide) {
							ushort id = br.ReadUInt16();
							if (id == contaId) {
								using (BinaryWriter bw = new BinaryWriter(fs)) {
									bw.BaseStream.Position = pos;
									bw.Write(true);
									return true;
								}
							} else {
								br.BaseStream.Position += tam - 2;
							}
						} else {
							br.BaseStream.Position += tam;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Retorna uma <see cref="ContaBancaria"/> requisitada com base no <paramref name="contaId"></paramref>
		/// </summary>
		/// <param name="contaId">ID a ser procurado no Banco de Dados</param>
		/// <returns>Uma <see cref="ContaBancaria"/> com os dados encontrados no Banco de Dados</returns>
		public ContaBancaria? ReadById(ushort contaId) {
			ContaBancaria conta = new ContaBancaria();
			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.Read)) {
				long length = fs.Length;
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Seek(2, SeekOrigin.Begin);
					while (br.PeekChar() != -1) {
						conta.Lapide = br.ReadBoolean();
						int tam = br.ReadInt32();

						conta.Deserialize(br);

						if (conta.IdConta == contaId) {
							return conta;
						} else {
							br.BaseStream.Position += tam;
						}
					}
				}
			}
			return null;
		}

		public ContaBancaria? ReadByPos(long pos) {
			ContaBancaria conta = new ContaBancaria();
			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Position = pos;
					conta.Lapide = br.ReadBoolean();
					br.ReadInt32();
					conta.Deserialize(br);
					return conta;
				}
			}
		}

		/// <summary>
		/// Faz o update de uma ContaBancaria no Banco de Dados.
		/// </summary>
		/// <param name="contaNova">Nova conta a ser substituída pela existente no Banco de Dados.</param>
		public void UpdateById(ContaBancaria contaNova, ushort contaId) {

			contaNova.IdConta = contaId;
			byte[] buffer = contaNova.Serialize();
			long pos;

			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.ReadWrite)) {
				using (BinaryReader br = new BinaryReader(fs)) {
					br.BaseStream.Seek(2, SeekOrigin.Begin);
					while (br.PeekChar() != -1) {
						bool lapide = br.ReadBoolean();
						int tam = br.ReadInt32();

						pos = br.BaseStream.Position;
						ushort id = br.ReadUInt16();

						if (id == contaId) {
							if (tam < buffer.Length) {
								DeleteById(contaId);
								Create(contaNova);
								break;
							} else {
								using (BinaryWriter bw = new BinaryWriter(fs)) {
									bw.BaseStream.Position = pos;
									bw.Write(buffer);
								}
								break;
							}
						} else {
							br.BaseStream.Position += tam - 2;
						}
					}
				}
			}
		}

		/// <summary>
		/// Faz o update do ID (último ID) no início do arquivo com base no ID do último registro inserido.
		/// </summary>
		/// <param name="id">ID do último registro</param>
		/// <returns>Último ID</returns>
		public void UpdateMaxId(ushort id) {
			using (FileStream fs = new FileStream(pathContaDB, FileMode.Open)) {
				using (BinaryWriter bw = new BinaryWriter(fs)) {
					bw.Seek(0, SeekOrigin.Begin);
					bw.Write(id);
				}
			}
		}

		/// <summary>
		/// Cria o arquivo inicial Conta.db para ser usado como Banco de Dados (Arquivo Sequencial) temporário
		/// </summary>
		public void BuildFile() {

			// Cria o arquivo de indíces
			BuildIndexFile();

			// Verifica se o arquivo existe ou não. Caso não exista, cria o arquivo e coloca o maxId como 0.
			// Caso o arquivo já exista, verifica se o arquivo só contém o maxId escrito nele e se ele é igual a zero e reescreve
			if (!File.Exists(pathContaDB)) {
				using (FileStream fs = new FileStream(pathContaDB, FileMode.Create)) {
					using (BinaryWriter bw = new BinaryWriter(fs)) {
						bw.Seek(0, SeekOrigin.Begin);
						bw.Write((ushort)0);
					}
				}
			} else {
				using (FileStream fs = new FileStream(pathContaDB, FileMode.Open, FileAccess.ReadWrite)) {
					using (BinaryReader br = new BinaryReader(fs)) {
						br.BaseStream.Seek(0, SeekOrigin.Begin);
						long length = fs.Length;
						if (length == 0) {
							using (BinaryWriter bw = new BinaryWriter(fs)) {
								bw.Write((ushort)0);
							}
						}
					}
				}
			}
		}

		public void BuildIndexFile() {
			if (!File.Exists(pathIndexDB)) {
				File.Create(pathIndexDB);
			}
		}

		public void CreateIndex(ushort id, long pos) {
			using (FileStream fs = new FileStream(pathIndexDB, FileMode.Open, FileAccess.Write)) {
				using (BinaryWriter bw = new BinaryWriter(fs)) {
					bw.Seek(0, SeekOrigin.End);
					bw.Write(id);
					bw.Write(pos);
				}
			}
		}

public long FindPosByIndex(ushort index) {
	using (FileStream fs = new FileStream(pathIndexDB, FileMode.Open, FileAccess.Read)) {
		using (BinaryReader br = new BinaryReader(fs)) {
			long len = br.BaseStream.Length, inf = 0, sup = len, meio;
			br.BaseStream.Seek(0, SeekOrigin.Begin);
			while (br.PeekChar() != -1 && inf <= sup) {
				meio = (inf + sup) / 2;

				if (meio % 10 <= 5) {
					meio -= meio % 10;
				} else {
					meio += meio % 10;
				}

				br.BaseStream.Position = meio;
				Console.WriteLine("Meio: " + meio);

				ushort id = br.ReadUInt16();
				Console.WriteLine("ID: " + id);
				if (id == index) {
					return br.ReadInt64();
				} else if (id > index) {
					sup = meio;
				} else {
					inf = meio;
				}
				Console.WriteLine("Sup: " + sup);
				Console.WriteLine("Inf: " + inf);
			}
		}
	}
	return -1;
}
	}
}
