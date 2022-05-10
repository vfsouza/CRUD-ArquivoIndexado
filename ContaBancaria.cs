using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trabalho1.Environment;

namespace Trabalho1.Models {
	[Serializable]
	public class ContaBancaria {
		public ushort IdConta { get; set; }
		public string NomePessoa { get; set; }
		public string CPF { get; set; }
		public string Cidade { get; set; }
		public ushort TransfRealizadas { get; set; }
		public float SaldoConta { get; set; }
		public bool Lapide { get; set; }

		// Constructors

		// DEFAULT
		public ContaBancaria() {
			IdConta = 0;
			NomePessoa = "";
			CPF = "";
			Cidade = "";
			TransfRealizadas = 0;
			SaldoConta = 0;
			Lapide = false;
		}

		public ContaBancaria(string nomePessoa, string cpf, string cidade, ushort transfRealizadas, uint saldoConta) {
			IdConta = 0;
			NomePessoa = nomePessoa;
			CPF = cpf;
			Cidade = cidade;
			TransfRealizadas = transfRealizadas;
			SaldoConta = saldoConta;
			Lapide = false;
		}

		/// <summary>
		/// Marca o atributo Lápide de conta como <see langword="true"/>.
		/// </summary>
		public void DeleteConta() {
			Lapide = true;
		}

		/// <summary>
		/// Atualiza o saldo da conta e o número de transferências realizadas.
		/// </summary>
		/// <param name="saldoNovo">Novo saldo da conta</param>
		public void HandleTransferencia(uint saldoNovo) {
			SaldoConta = saldoNovo;
			TransfRealizadas++;
		}

		/// <summary>
		/// Serializa o objeto <see cref="ContaBancaria"/>.
		/// </summary>
		/// <returns>Buffer de <see cref="byte"/> dos atributos do objeto serializado</returns>
		public byte[] Serialize() {
			using (MemoryStream ms = new MemoryStream()) {
				using (BinaryWriter bw = new BinaryWriter(ms)) {
					bw.Write(IdConta);
					bw.Write(NomePessoa);
					bw.Write(CPF);
					bw.Write(Cidade);
					bw.Write(TransfRealizadas);
					bw.Write(SaldoConta);
				}
				return ms.ToArray();
			}
		}

		/// <summary>
		/// Desserializa o objeto através de um buffer de <see cref="byte"/>.
		/// </summary>
		/// <param name="buffer">Array de <see cref="byte"/> à ser desserializado.</param>
		public void Deserialize(byte[] buffer) {
			using (MemoryStream ms = new MemoryStream(buffer)) {
				using (BinaryReader br = new BinaryReader(ms)) {
					IdConta = br.ReadUInt16();
					NomePessoa = br.ReadString();
					CPF = br.ReadString();
					Cidade = br.ReadString();
					TransfRealizadas = br.ReadUInt16();
					SaldoConta = br.ReadSingle();
				}
			}
		}

		/// <summary>
		/// Desserializa o objeto através de um <see cref="BinaryReader"/> (<paramref name="br"/>) já inicializado.
		/// </summary>
		/// <param name="br"><see cref="BinaryReader"/> inicializado para leitura de uma <see cref="Stream"/></param>
		public void Deserialize(BinaryReader br) {
			IdConta = br.ReadUInt16();
			NomePessoa = br.ReadString();
			CPF = br.ReadString();
			Cidade = br.ReadString();
			TransfRealizadas = br.ReadUInt16();
			SaldoConta = br.ReadSingle();
		}
		

		/// <summary>
		/// Transforma a classe e seus dados em uma única string.
		/// </summary>
		/// <returns>Uma <see cref="string"/> com os dados da classe</returns>
		public override string ToString() {
			return $"ID......> {IdConta}\n" +
				   $"NOME....> {NomePessoa}\n" +
				   $"CPF.....> {CPF}\n" +
				   $"CIDADE..> {Cidade}\n" +
				   $"TRANSF..> {TransfRealizadas}\n" +
				   $"SALDO...> {SaldoConta}\n" +
				   $"LAPIDE..> {Lapide}\n";
		}

	}
}
